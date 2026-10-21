using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AllocatrApi.Services;

public sealed class MessagingException : Exception
{
    public int StatusCode { get; }
    public MessagingException(int statusCode, string message) : base(message) => StatusCode = statusCode;
}

public class ProjectMessagingService
{
    public const long MaxRequestBytes = 52L * 1024 * 1024;
    public const long MaxCombinedFileBytes = 50L * 1024 * 1024;
    private readonly AllocatrDbContext _db;
    private readonly ProjectAccessService _access;
    private readonly ProjectFileValidationService _validation;
    private readonly IFileStorageService _storage;
    private readonly ILogger<ProjectMessagingService> _logger;

    public ProjectMessagingService(AllocatrDbContext db, ProjectAccessService access,
        ProjectFileValidationService validation, IFileStorageService storage,
        ILogger<ProjectMessagingService> logger)
    {
        _db = db; _access = access; _validation = validation; _storage = storage; _logger = logger;
    }

    private IQueryable<ProjectConversation> Visible(Guid projectId, Guid userId) =>
        _db.ProjectConversations.Where(c => c.ProjectId == projectId &&
            (c.IsProjectWide || c.Participants.Any(p => p.UserId == userId)));

    private async Task RequireProject(Guid projectId, Guid userId, bool isAllocat)
    {
        if (userId == Guid.Empty || !await _access.CanViewProjectAsync(projectId, userId, isAllocat))
            throw new MessagingException(404, "Project not found or not accessible.");
    }

    public async Task<List<ConversationMemberDto>> MembersAsync(Guid projectId, Guid userId,
        bool isAllocat, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        var owner = await _db.Projects.AsNoTracking().Where(p => p.Id == projectId)
            .Select(p => new ConversationMemberDto(p.UserId, p.User.FullName ?? "Project owner", p.User.AvatarUrl, true))
            .SingleAsync(ct);
        var members = await _db.ProjectAllocats.AsNoTracking()
            .Where(p => p.ProjectId == projectId && p.Status == ProjectAllocatStatus.Accepted && p.RemovedAt == null
                && p.AllocatProfile.AllocatrUser.IsAllocat)
            .Select(p => new ConversationMemberDto(p.AllocatProfile.AllocatrUserId,
                p.AllocatProfile.AllocatrUser.FullName ?? "Project member", p.AllocatProfile.AllocatrUser.AvatarUrl, false))
            .ToListAsync(ct);
        return members.Prepend(owner).DistinctBy(p => p.UserId).OrderByDescending(p => p.IsOwner).ThenBy(p => p.Name).ToList();
    }

    public async Task<ConversationListDto> ListAsync(Guid projectId, Guid userId, bool isAllocat,
        string? search, string? filter, int page, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        var visible = Visible(projectId, userId).AsNoTracking();
        var unreadTotal = await visible.Where(c => !c.Participants.Any(p => p.UserId == userId && p.IsArchived))
            .SelectMany(c => c.Messages.Where(m => m.DeletedAt == null && m.SenderId != userId && m.Sequence >
                (c.Participants.Where(p => p.UserId == userId).Select(p => (long?)p.LastReadSequence).FirstOrDefault() ?? 0)))
            .CountAsync(ct);
        var query = visible;
        switch (filter ?? "inbox")
        {
            case "inbox": query = query.Where(c => !c.Participants.Any(p => p.UserId == userId && p.IsArchived)); break;
            case "unread":
                query = query.Where(c => !c.Participants.Any(p => p.UserId == userId && p.IsArchived) &&
                c.Messages.Any(m => m.DeletedAt == null && m.SenderId != userId && m.Sequence >
                    (c.Participants.Where(p => p.UserId == userId).Select(p => (long?)p.LastReadSequence).FirstOrDefault() ?? 0))); break;
            case "archived": query = query.Where(c => c.Participants.Any(p => p.UserId == userId && p.IsArchived)); break;
            default: throw new MessagingException(400, "Choose Inbox, Unread or Archived.");
        }
        var term = search?.Trim().ToLowerInvariant() ?? "";
        if (term.Length > 100) throw new MessagingException(400, "Search cannot exceed 100 characters.");
        if (term.Length > 0) query = query.Where(c => c.Subject.ToLower().Contains(term) || c.Messages.Any(m => m.DeletedAt == null && (m.Body.ToLower().Contains(term) || (m.ForwardNote != null && m.ForwardNote.ToLower().Contains(term)))));
        var count = await query.CountAsync(ct);
        page = Math.Clamp(page, 1, 100000);
        var items = await query.OrderByDescending(c => c.Participants.Any(p => p.UserId == userId && p.IsPinned))
            .ThenByDescending(c => c.UpdatedAt).ThenByDescending(c => c.Id)
            .Skip((page - 1) * 30).Take(30)
            .Select(c => new ConversationSummaryDto(c.Id, c.Subject, c.IsProjectWide, c.UpdatedAt,
                c.Messages.OrderByDescending(m => m.Sequence).Select(m => m.DeletedAt != null ? "This message was deleted" : m.Body.Length > 160 ? m.Body.Substring(0, 160) : m.Body).FirstOrDefault() ?? "",
                c.Messages.Count(m => m.DeletedAt == null && m.SenderId != userId && m.Sequence >
                    (c.Participants.Where(p => p.UserId == userId).Select(p => (long?)p.LastReadSequence).FirstOrDefault() ?? 0)),
                c.LastSequence, c.Participants.Any(p => p.UserId == userId && p.IsArchived), c.TaskId, c.TaskTitle, c.Participants.Any(p => p.UserId == userId && p.IsPinned),
                c.Messages.Any(m => m.DeletedAt == null && m.Attachments.Any(a => a.StoredFile.Status == StoredFileStatus.Ready))))
            .ToListAsync(ct);
        return new ConversationListDto(items, page, count, unreadTotal);
    }

    public async Task<ConversationDetailDto> DetailAsync(Guid projectId, Guid conversationId, Guid userId,
        bool isAllocat, long? before, CancellationToken ct, bool canModerate = false)
    {
        await RequireProject(projectId, userId, isAllocat);
        var c = await Visible(projectId, userId).AsNoTracking().Include(c => c.Participants).ThenInclude(p => p.User)
            .SingleOrDefaultAsync(c => c.Id == conversationId, ct)
            ?? throw new MessagingException(404, "Conversation not found.");
        var messages = await _db.ConversationMessages.AsNoTracking()
            .Where(m => m.ConversationId == c.Id && (!before.HasValue || m.Sequence < before.Value))
            .OrderByDescending(m => m.Sequence).Take(51).Include(m => m.Sender)
            .Include(m => m.Attachments).ThenInclude(a => a.StoredFile).AsSplitQuery().ToListAsync(ct);
        var members = c.IsProjectWide ? await MembersAsync(projectId, userId, isAllocat, ct)
            : c.Participants.Select(p => new ConversationMemberDto(p.UserId, p.User.FullName ?? "Project member", p.User.AvatarUrl, false)).OrderBy(p => p.Name).ToList();
        return new ConversationDetailDto(c.Id, c.Subject, c.IsProjectWide,
            c.Participants.Any(p => p.UserId == userId && p.IsArchived), c.TaskId, c.TaskTitle, members,
            messages.Take(50).Reverse().Select(m => MessageDto(m, userId, canModerate)).ToList(),
            messages.Count > 50, c.LastSequence, c.Participants.Any(p => p.UserId == userId && p.IsPinned));
    }

    public async Task<ConversationSendResultDto> CreateAsync(Guid projectId, Guid userId, bool isAllocat,
        CreateConversationForm form, CancellationToken ct) =>
        await SendAsync(projectId, null, userId, isAllocat, form.Subject, form.IsProjectWide,
            form.RecipientIds, form.TaskId, form.Body, form.ClientRequestId, form.Files, ct);

    public async Task<ConversationSendResultDto> ReplyAsync(Guid projectId, Guid conversationId, Guid userId,
        bool isAllocat, ReplyConversationForm form, CancellationToken ct) =>
        await SendAsync(projectId, conversationId, userId, isAllocat, "", false, [], null,
            form.Body, form.ClientRequestId, form.Files, ct);

    private async Task<ConversationSendResultDto> SendAsync(Guid projectId, Guid? conversationId, Guid userId,
        bool isAllocat, string subject, bool projectWide, List<Guid> recipientIds, Guid? taskId, string body,
        Guid nonce, List<IFormFile> files, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        body = (body ?? "").Trim(); subject = (subject ?? "").Trim();
        if (nonce == Guid.Empty) throw new MessagingException(400, "A request ID is required.");
        if (body.Length > 10000 || (body.Length == 0 && files.Count == 0))
            throw new MessagingException(400, "Write a message or attach a file. Messages can contain up to 10,000 characters.");
        if (files.Count > 8 || files.Sum(f => f.Length) > MaxCombinedFileBytes)
            throw new MessagingException(400, "Attach up to 8 files, with a combined size of 50 MB or less.");
        var recipients = recipientIds.Distinct().Where(id => id != userId).OrderBy(id => id).ToArray();
        if (!conversationId.HasValue && (subject.Length == 0 || subject.Length > 200))
            throw new MessagingException(400, "Enter a subject of up to 200 characters.");
        if (!conversationId.HasValue && (recipients.Length > 100 || (!projectWide && recipients.Length == 0)))
            throw new MessagingException(400, "Select at least one other project member, up to 100 recipients.");
        if (projectWide && recipients.Length > 0) throw new MessagingException(400, "Choose everyone or specific people, not both.");
        // Validate the actual bytes using the existing bounded file validator, before any storage writes.
        var uploads = new List<ValidatedProjectUpload>();
        foreach (var file in files) uploads.Add(await _validation.ValidateAndReadAsync(file, ct));
        if (uploads.Sum(f => (long)f.Bytes.Length) > MaxCombinedFileBytes)
            throw new MessagingException(400, "Attachments cannot exceed 50 MB combined.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            projectId,
            conversationId,
            subject,
            projectWide,
            recipients,
            taskId,
            body,
            files = uploads.Select(f => new { f.OriginalFileName, f.ContentType, Hash = Convert.ToHexString(SHA256.HashData(f.Bytes)) })
        }))));
        var replay = await ReplayAsync(projectId, conversationId, userId, isAllocat, nonce, hash, ct);
        if (replay != null) return replay;
        await ValidateAudience(projectId, conversationId, userId, isAllocat, recipients, ct);
        string? taskTitle = null;
        if (taskId.HasValue)
            taskTitle = await _db.TaskItems.AsNoTracking().Where(t => t.Id == taskId && t.ProjectId == projectId)
                .Select(t => t.Title).SingleOrDefaultAsync(ct) ?? throw new MessagingException(400, "Choose a task from this project.");
        var storedFiles = new List<StoredFile>();
        try
        {
            foreach (var upload in uploads)
            {
                var stored = new StoredFile
                {
                    Id = Guid.NewGuid(),
                    UploadedByUserId = userId,
                    Bucket = ProjectFileService.Bucket,
                    OriginalFileName = upload.OriginalFileName,
                    ContentType = upload.ContentType,
                    SizeBytes = upload.Bytes.LongLength,
                    Status = StoredFileStatus.Uploading,
                    CreatedAt = DateTime.UtcNow
                };
                stored.StoragePath = $"projects/{projectId}/messages/{stored.Id}{upload.Extension}";
                _db.StoredFiles.Add(stored);
                await _db.SaveChangesAsync(ct);
                storedFiles.Add(stored);
                await _storage.UploadAsync(stored.Bucket, stored.StoragePath, upload.Bytes, upsert: false);
            }
            // Membership may have changed during a slow upload. Private recipients must still be current members.
            await ValidateAudience(projectId, conversationId, userId, isAllocat, recipients, ct);
            ProjectConversation c;
            var now = DateTime.UtcNow;
            if (conversationId.HasValue)
            {
                c = await Visible(projectId, userId).Include(x => x.Participants).SingleAsync(x => x.Id == conversationId, ct);
                await _db.Entry(c).ReloadAsync(ct);
            }
            else
            {
                c = new ProjectConversation
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    CreatedById = userId,
                    Subject = subject,
                    IsProjectWide = projectWide,
                    CreatedAt = now,
                    TaskId = taskId,
                    TaskTitle = taskTitle
                };
                foreach (var id in recipients.Append(userId)) c.Participants.Add(new ConversationParticipant { UserId = id });
                _db.ProjectConversations.Add(c);
            }
            c.LastSequence++; c.UpdatedAt = now;
            // A new reply returns existing participants' archived threads to their inboxes.
            foreach (var participant in c.Participants) participant.IsArchived = false;
            var message = new ConversationMessage
            {
                Id = Guid.NewGuid(),
                Conversation = c,
                SenderId = userId,
                Sequence = c.LastSequence,
                Body = body,
                CreatedAt = now,
                ClientRequestId = nonce,
                RequestHash = hash
            };
            foreach (var stored in storedFiles)
            {
                stored.Status = StoredFileStatus.Ready; stored.ReadyAt = now;
                message.Attachments.Add(new ConversationAttachment { Id = Guid.NewGuid(), StoredFileId = stored.Id });
            }
            _db.ConversationMessages.Add(message);
            // One SaveChanges transaction publishes the message, all attachments, and sequence together.
            await _db.SaveChangesAsync(ct);
            return new ConversationSendResultDto(c.Id, message.Id);
        }
        catch (Exception error)
        {
            _db.ChangeTracker.Clear();
            // If the client timed out during a successful commit, do not delete committed attachments.
            ConversationSendResultDto? committed = null;
            MessagingException? replayError = null;
            try { committed = await ReplayAsync(projectId, conversationId, userId, isAllocat, nonce, hash, CancellationToken.None); }
            catch (MessagingException denied) { replayError = denied; }
            catch (Exception recoveryError)
            {
                // Database availability is uncertain. Leave metadata for recovery rather than risk
                // deleting attachments whose final commit may have succeeded.
                _logger.LogError(recoveryError, "Message send recovery required for request {RequestId}", nonce);
                throw new MessagingException(503, "Message delivery could not be confirmed. Retry the unchanged draft.");
            }
            await CleanupUnboundAsync(storedFiles.Select(f => f.Id).ToArray());
            if (committed != null) return committed;
            if (replayError != null) throw replayError;
            if (error is DbUpdateConcurrencyException || error is DbUpdateException { InnerException: PostgresException { SqlState: "23505" } })
                throw new MessagingException(409, "Another reply arrived while sending. Please press Send again.");
            throw;
        }
    }

    private async Task ValidateAudience(Guid projectId, Guid? conversationId, Guid userId, bool isAllocat,
        Guid[] recipients, CancellationToken ct)
    {
        var members = await MembersAsync(projectId, userId, isAllocat, ct);
        if (conversationId.HasValue)
        {
            if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
                throw new MessagingException(404, "Conversation not found.");
        }
        else if (recipients.Any(id => !members.Any(m => m.UserId == id)))
            throw new MessagingException(400, "One of the selected people is no longer a member of this project.");
    }

    private async Task<ConversationSendResultDto?> ReplayAsync(Guid projectId, Guid? conversationId,
        Guid userId, bool isAllocat, Guid nonce, string hash, CancellationToken ct)
    {
        var existing = await _db.ConversationMessages.AsNoTracking()
            .Where(m => m.SenderId == userId && m.ClientRequestId == nonce)
            .Select(m => new { m.Id, m.ConversationId, m.RequestHash }).SingleOrDefaultAsync(ct);
        if (existing == null) return null;
        await RequireProject(projectId, userId, isAllocat);
        if (existing.RequestHash != hash || (conversationId.HasValue && existing.ConversationId != conversationId)
            || !await Visible(projectId, userId).AnyAsync(c => c.Id == existing.ConversationId, ct))
            throw new MessagingException(409, "This request ID was already used for a different message.");
        return new ConversationSendResultDto(existing.ConversationId, existing.Id);
    }

    private async Task CleanupUnboundAsync(Guid[] ids)
    {
        foreach (var id in ids)
        {
            try
            {
                // Never remove files attached by a committed request, including duplicate-request races.
                if (await _db.ConversationAttachments.AnyAsync(a => a.StoredFileId == id)) continue;
                var file = await _db.StoredFiles.SingleOrDefaultAsync(f => f.Id == id);
                if (file == null) continue;
                file.Status = StoredFileStatus.Failed;
                await _db.SaveChangesAsync(CancellationToken.None);
                await _storage.DeleteAsync(file.Bucket, file.StoragePath);
            }
            catch (Exception error) { _logger.LogError(error, "Message attachment cleanup required for {StoredFileId}", id); }
        }
    }

    public async Task MarkReadAsync(Guid projectId, Guid conversationId, Guid userId, bool isAllocat, long through, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        var last = await Visible(projectId, userId).AsNoTracking().Where(c => c.Id == conversationId)
            .Select(c => (long?)c.LastSequence).SingleOrDefaultAsync(ct) ?? throw new MessagingException(404, "Conversation not found.");
        if (through < 0 || through > last) throw new MessagingException(400, "Invalid read position.");
        // Atomic upsert: concurrent tabs must never move a read position backwards.
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ConversationParticipants" ("ConversationId", "UserId", "LastReadSequence", "IsArchived")
            VALUES ({conversationId}, {userId}, {through}, FALSE)
            ON CONFLICT ("ConversationId", "UserId") DO UPDATE
            SET "LastReadSequence" = GREATEST("ConversationParticipants"."LastReadSequence", EXCLUDED."LastReadSequence")
            """, ct);
    }

    public async Task ArchiveAsync(Guid projectId, Guid conversationId, Guid userId, bool isAllocat, bool archived, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
            throw new MessagingException(404, "Conversation not found.");
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ConversationParticipants" ("ConversationId", "UserId", "LastReadSequence", "IsArchived")
            VALUES ({conversationId}, {userId}, 0, {archived})
            ON CONFLICT ("ConversationId", "UserId") DO UPDATE SET "IsArchived" = EXCLUDED."IsArchived"
            """, ct);
    }

    public async Task PinAsync(Guid projectId, Guid conversationId, Guid userId, bool isAllocat,
        bool pinned, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
            throw new MessagingException(404, "Conversation not found.");
        // Personal preference, not a shared conversation mutation. Preserve read/archive
        // state on conflict, including when different browser tabs update them concurrently.
        await _db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT INTO "ConversationParticipants" ("ConversationId", "UserId", "LastReadSequence", "IsArchived", "IsPinned")
            VALUES ({conversationId}, {userId}, 0, FALSE, {pinned})
            ON CONFLICT ("ConversationId", "UserId") DO UPDATE SET "IsPinned" = EXCLUDED."IsPinned"
            """, ct);
    }

    public const int ActionWindowMinutes = 15;

    private static ConversationMessageDto MessageDto(ConversationMessage m, Guid userId, bool canModerate)
    {
        var available = m.DeletedAt == null;
        var expires = m.CreatedAt.AddMinutes(ActionWindowMinutes);
        var ownWindow = m.SenderId == userId && DateTime.UtcNow < expires;
        return new ConversationMessageDto(m.Id, m.Sequence, m.SenderId,
            m.Sender.FullName ?? "Project member", m.Sender.AvatarUrl,
            available ? m.Body : "", m.CreatedAt,
            available ? m.Attachments.Where(a => a.StoredFile.Status == StoredFileStatus.Ready)
                .Select(a => new ConversationFileDto(a.Id, a.StoredFile.OriginalFileName,
                    a.StoredFile.ContentType, a.StoredFile.SizeBytes)).ToList() : [],
            m.EditedAt, m.DeletedAt, m.Version, expires,
            available && ownWindow, available && (ownWindow || canModerate), canModerate,
            available ? m.ForwardedFromName : null, available ? m.ForwardedCreatedAt : null,
            available ? m.ForwardNote : null);
    }

    // Call inside a transaction. Serialize edit/delete/forward for this source message.
    private async Task<ConversationMessage> LockMessage(Guid projectId, Guid conversationId, Guid messageId,
        Guid userId, bool isAllocat, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
            throw new MessagingException(404, "Conversation not found.");
        var rows = await _db.ConversationMessages.FromSqlInterpolated($"""
            SELECT * FROM "ConversationMessages"
            WHERE "Id" = {messageId} AND "ConversationId" = {conversationId} FOR UPDATE
            """).ToListAsync(ct);
        var message = rows.SingleOrDefault() ?? throw new MessagingException(404, "Message not found.");
        await _db.Entry(message).Reference(m => m.Sender).LoadAsync(ct);
        await _db.Entry(message).Collection(m => m.Attachments).Query().Include(a => a.StoredFile).LoadAsync(ct);
        return message;
    }

    public async Task EditAsync(Guid projectId, Guid conversationId, Guid messageId, Guid userId,
        bool isAllocat, ConversationEditDto form, CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        var m = await LockMessage(projectId, conversationId, messageId, userId, isAllocat, ct);
        if (m.SenderId != userId) throw new MessagingException(403, "Only the sender can edit this message.");
        if (m.DeletedAt != null || DateTime.UtcNow >= m.CreatedAt.AddMinutes(ActionWindowMinutes))
            throw new MessagingException(409, "This message can no longer be edited.");
        if (m.Version != form.Version) throw new MessagingException(409, "This message changed. Refresh before editing.");
        var body = (form.Body ?? "").Trim();
        if (body.Length > 10000 || (body.Length == 0 && m.ForwardedFromName == null && !m.Attachments.Any(a => a.StoredFile.Status == StoredFileStatus.Ready)))
            throw new MessagingException(400, "Write a message of up to 10,000 characters or keep an attachment.");
        // A forwarded snapshot stays intact; only its accompanying note is editable.
        if (m.ForwardedFromName != null) m.ForwardNote = body;
        else m.Body = body;
        m.EditedAt = DateTime.UtcNow; m.Version++;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task DeleteAsync(Guid projectId, Guid conversationId, Guid messageId, Guid userId,
        bool isAllocat, bool canModerate, ConversationDeleteDto form, CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        var m = await LockMessage(projectId, conversationId, messageId, userId, isAllocat, ct);
        if (m.SenderId != userId && !canModerate) throw new MessagingException(403, "Only the sender can delete this message.");
        if (m.DeletedAt != null) { await transaction.CommitAsync(ct); return; }
        if (!canModerate && DateTime.UtcNow >= m.CreatedAt.AddMinutes(ActionWindowMinutes))
            throw new MessagingException(409, "This message can no longer be deleted.");
        if (m.Version != form.Version) throw new MessagingException(409, "This message changed. Refresh before deleting.");
        m.DeletedAt = DateTime.UtcNow; m.DeletedById = userId; m.Version++;
        // Keep a tombstone and audit metadata, never return the deleted body or attachments.
        m.Body = ""; m.ForwardNote = null; m.ForwardedFromName = null; m.ForwardedCreatedAt = null;
        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        // Stored bytes may be referenced by an independently forwarded copy. Do not mark the
        // shared StoredFile Deleted or remove it here; download permission follows each message.
    }

    public async Task<ConversationSendResultDto> ForwardAsync(Guid projectId, Guid conversationId,
        Guid messageId, Guid userId, bool isAllocat, ConversationForwardDto form, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        if (form.ClientRequestId == Guid.Empty) throw new MessagingException(400, "A request ID is required.");
        var recipients = (form.RecipientIds ?? []).Distinct().Where(x => x != userId).OrderBy(x => x).ToArray();
        var subject = (form.Subject ?? "").Trim(); var note = (form.Note ?? "").Trim();
        if (subject.Length == 0 || subject.Length > 200 || note.Length > 10000 || recipients.Length > 100
            || (!form.IsProjectWide && recipients.Length == 0) || (form.IsProjectWide && recipients.Length > 0))
            throw new MessagingException(400, "Choose a subject, project recipients, and an optional note of up to 10,000 characters.");
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            operation = "forward",
            projectId,
            conversationId,
            messageId,
            form.Version,
            subject,
            note,
            form.IsProjectWide,
            recipients
        }))));
        // Validate source visibility even for a replay; no access to a private source is implied by a target.
        if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
            throw new MessagingException(404, "Conversation not found.");
        var replay = await ReplayAsync(projectId, null, userId, isAllocat, form.ClientRequestId, hash, ct);
        if (replay != null) return replay;
        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            var m = await LockMessage(projectId, conversationId, messageId, userId, isAllocat, ct);
            if (m.DeletedAt != null || m.Version != form.Version)
                throw new MessagingException(409, "The source message changed. Refresh before forwarding.");
            await ValidateAudience(projectId, null, userId, isAllocat, recipients, ct);
            var now = DateTime.UtcNow;
            var c = new ProjectConversation
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                CreatedById = userId,
                Subject = subject,
                IsProjectWide = form.IsProjectWide,
                CreatedAt = now,
                UpdatedAt = now,
                LastSequence = 1
            };
            foreach (var person in recipients.Append(userId)) c.Participants.Add(new ConversationParticipant { UserId = person });
            var snapshot = m.ForwardedFromName == null ? m.Body :
                $"{m.ForwardNote}\n\nForwarded from {m.ForwardedFromName} ({m.ForwardedCreatedAt:O})\n{m.Body}".Trim();
            if (snapshot.Length > 10000) throw new MessagingException(400, "This forwarded chain is too long. Forward the original message instead.");
            var copy = new ConversationMessage
            {
                Id = Guid.NewGuid(),
                Conversation = c,
                SenderId = userId,
                Sequence = 1,
                Body = snapshot,
                CreatedAt = now,
                ClientRequestId = form.ClientRequestId,
                RequestHash = hash,
                ForwardedFromName = (m.Sender.FullName ?? "Project member")[..Math.Min((m.Sender.FullName ?? "Project member").Length, 256)],
                ForwardedCreatedAt = m.CreatedAt,
                ForwardNote = note
            };
            foreach (var file in m.Attachments.Where(a => a.StoredFile.Status == StoredFileStatus.Ready))
                copy.Attachments.Add(new ConversationAttachment { Id = Guid.NewGuid(), StoredFileId = file.StoredFileId });
            _db.ConversationMessages.Add(copy);
            await _db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            return new ConversationSendResultDto(c.Id, copy.Id);
        }
        catch (Exception error) when (error is DbUpdateException || error is System.IO.IOException || error is OperationCanceledException)
        {
            _db.ChangeTracker.Clear();
            var committed = await ReplayAsync(projectId, null, userId, isAllocat, form.ClientRequestId, hash, CancellationToken.None);
            if (committed != null) return committed;
            throw new MessagingException(409, "Forwarding could not be confirmed. Retry the unchanged draft.");
        }
    }

    public async Task<(byte[] Bytes, string ContentType, string Name)> DownloadAsync(Guid projectId, Guid conversationId, Guid fileId,
        Guid userId, bool isAllocat, IHttpClientFactory httpClients, CancellationToken ct)
    {
        await RequireProject(projectId, userId, isAllocat);
        if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct))
            throw new MessagingException(404, "Conversation not found.");
        var file = await _db.ConversationAttachments.AsNoTracking()
            .Where(a => a.Id == fileId && a.Message.ConversationId == conversationId && a.Message.DeletedAt == null && a.StoredFile.Status == StoredFileStatus.Ready)
            .Select(a => new { a.StoredFile.Bucket, a.StoredFile.StoragePath, a.StoredFile.ContentType, a.StoredFile.OriginalFileName }).SingleOrDefaultAsync(ct)
            ?? throw new MessagingException(404, "Attachment not found.");
        try
        {
            var url = await _storage.CreateSignedUrlAsync(file.Bucket, file.StoragePath, 60);
            if (string.IsNullOrWhiteSpace(url)) throw new IOException("Empty signed URL.");
            using var client = httpClients.CreateClient("MessagingAttachmentSource");
            using var response = await client.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > 25L * 1024 * 1024) throw new IOException("Attachment is too large.");
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var buffer = new MemoryStream();
            var chunk = new byte[65536]; int read;
            while ((read = await stream.ReadAsync(chunk.AsMemory(), ct)) > 0)
            {
                if (buffer.Length + read > 25L * 1024 * 1024) throw new IOException("Attachment is too large.");
                await buffer.WriteAsync(chunk.AsMemory(0, read), ct);
            }
            // Recheck after storage I/O so a message deleted during the fetch is denied.
            await RequireProject(projectId, userId, isAllocat);
            if (!await Visible(projectId, userId).AnyAsync(c => c.Id == conversationId, ct)
                || !await _db.ConversationMessages.AnyAsync(m => m.ConversationId == conversationId
                    && m.DeletedAt == null && m.Attachments.Any(a => a.Id == fileId), ct))
                throw new MessagingException(404, "Attachment not found.");
            return (buffer.ToArray(), file.ContentType, file.OriginalFileName);
        }
        catch (MessagingException) { throw; }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception error)
        {
            _logger.LogError(error, "Could not download message attachment {AttachmentId}", fileId);
            throw new MessagingException(503, "This attachment is temporarily unavailable. Please try again.");
        }
    }
}
