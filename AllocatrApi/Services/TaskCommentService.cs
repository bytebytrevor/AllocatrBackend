using AllocatrApi.Constants;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class TaskCommentService
{
    public const int MaxAttachments = 8;
    private readonly AllocatrDbContext _db;
    private readonly ProjectAccessService _projectAccess;
    private readonly CollaborationNotificationService _notifications;

    public TaskCommentService(AllocatrDbContext db, ProjectAccessService projectAccess, CollaborationNotificationService notifications)
    {
        _db = db;
        _projectAccess = projectAccess;
        _notifications = notifications;
    }

    public async Task<TaskDiscussionContextDto?> GetContextAsync(Guid taskId, Guid userId, bool isAllocat)
    {
        var scope = await GetScopeAsync(taskId, userId, isAllocat);
        return scope == null ? null : new TaskDiscussionContextDto(scope.ProjectId,
            IsWritable(scope.Status), MaxAttachments, ProjectFileValidationService.MaxFileSizeBytes);
    }

    public async Task<List<TaskCommentDto>?> GetTaskCommentsAsync(Guid taskId, Guid userId, bool isAllocat)
    {
        var scope = await GetScopeAsync(taskId, userId, isAllocat);
        if (scope == null) return null;
        var comments = await _db.TaskComments.AsNoTracking().Include(comment => comment.CreatedBy)
            .Where(comment => comment.TaskItemId == taskId)
            .OrderBy(comment => comment.CreatedAt).ThenBy(comment => comment.Id).ToListAsync();
        return await MapAsync(comments, scope, userId);
    }

    public async Task<TaskCommentDto?> CreateCommentAsync(Guid taskId, Guid userId,
        bool isAllocat, CreateTaskCommentDto request)
    {
        var scope = await GetScopeAsync(taskId, userId, isAllocat);
        if (scope == null) return null;
        var text = request.Comment?.Trim() ?? string.Empty;
        var ids = (request.FileIds ?? []).ToArray();
        if (ids.Length > MaxAttachments || ids.Contains(Guid.Empty) || ids.Distinct().Count() != ids.Length)
            throw new ArgumentException($"Choose at most {MaxAttachments} different attachments.");
        ValidateComment(text, ids.Length > 0);
        if (request.ClientRequestId == Guid.Empty)
            throw new ArgumentException("The comment request ID is invalid.");

        if (request.ClientRequestId.HasValue)
        {
            var replay = await FindReplayAsync(request.ClientRequestId.Value, userId);
            if (replay != null) return await ReplayAsync(replay, taskId, text, ids, scope, userId, request.ReplyToCommentId);
        }
        if (ids.Length > 0 && !IsWritable(scope.Status))
            throw new ProjectFileException(409, "New attachments cannot be added while the project is awaiting completion or completed.");

        var comment = new TaskComment
        {
            Id = Guid.NewGuid(), TaskItemId = taskId, CreatedById = userId,
            Comment = text, CreatedAt = DateTime.UtcNow, ClientRequestId = request.ClientRequestId,
            ReplyToCommentId = request.ReplyToCommentId,
        };
        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync();
            Guid? replyAuthorId = null;
            if (request.ReplyToCommentId.HasValue)
            {
                replyAuthorId = await _db.TaskComments.AsNoTracking()
                    .Where(parent => parent.Id == request.ReplyToCommentId.Value && parent.TaskItemId == taskId)
                    .Select(parent => (Guid?)parent.CreatedById).SingleOrDefaultAsync();
                if (!replyAuthorId.HasValue)
                    throw new ArgumentException("The comment you are replying to is unavailable in this task.");
            }
            _db.TaskComments.Add(comment);
            await _db.SaveChangesAsync();
            if (ids.Length > 0)
            {
                // Conditional update claims each file exactly once. Recheck ready status,
                // uploader and both scopes; untrusted file IDs alone grant no access.
                var attached = await _db.ProjectFiles.Where(file => ids.Contains(file.Id)
                    && file.ProjectId == scope.ProjectId && file.TaskId == taskId
                    && file.UploadedByUserId == userId && file.CommentId == null
                    && file.Context == ProjectFileContext.Task
                    && file.StoredFile.Status == StoredFileStatus.Ready)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(file => file.CommentId, (Guid?)comment.Id)
                        .SetProperty(file => file.Context, ProjectFileContext.Comment));
                if (attached != ids.Length)
                    throw new ArgumentException("An attachment is unavailable, belongs to another task/user, or is already attached. No comment was posted.");
            }
            var notificationScope = await GetScopeAsync(taskId, userId, isAllocat)
                ?? throw new ProjectFileException(404, "Task not found or not accessible.");
            var actorName = await _db.Users.AsNoTracking().Where(user => user.Id == userId)
                .Select(user => user.FullName).SingleAsync() ?? "A project member";
            await _notifications.QueueCommentAsync(comment, notificationScope.ProjectId,
                notificationScope.ProjectTitle, notificationScope.TaskTitle, notificationScope.OwnerId,
                notificationScope.AssigneeId, replyAuthorId == notificationScope.OwnerId, actorName);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (DbUpdateException) when (request.ClientRequestId.HasValue)
        {
            // The transaction has been disposed/rolled back before this handler.
            // A unique nonce also covers concurrent retries of the same comment.
            _db.Entry(comment).State = EntityState.Detached;
            var replay = await FindReplayAsync(request.ClientRequestId.Value, userId);
            if (replay == null) throw;
            return await ReplayAsync(replay, taskId, text, ids, scope, userId, request.ReplyToCommentId);
        }
        return await ReadAsync(comment.Id, scope, userId);
    }

    public async Task<TaskCommentDto?> UpdateCommentAsync(Guid taskId, Guid commentId,
        Guid userId, bool isAllocat, string? comment)
    {
        var scope = await GetScopeAsync(taskId, userId, isAllocat);
        if (scope == null) return null;
        var existing = await _db.TaskComments.FirstOrDefaultAsync(item => item.Id == commentId
            && item.TaskItemId == taskId && item.CreatedById == userId);
        if (existing == null) return null;
        var text = comment?.Trim() ?? string.Empty;
        var hasAttachments = await _db.ProjectFiles.AnyAsync(file => file.CommentId == commentId
            && file.ProjectId == scope.ProjectId && file.StoredFile.Status == StoredFileStatus.Ready);
        ValidateComment(text, hasAttachments);
        existing.Comment = text;
        existing.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await ReadAsync(existing.Id, scope, userId);
    }

    public async Task<bool> DeleteCommentAsync(Guid taskId, Guid commentId, Guid userId, bool isAllocat)
    {
        if (await GetScopeAsync(taskId, userId, isAllocat) == null) return false;
        var comment = await _db.TaskComments.FirstOrDefaultAsync(item => item.Id == commentId
            && item.TaskItemId == taskId && item.CreatedById == userId);
        if (comment == null) return false;
        // FK SET NULL retains bytes and the task/project file record. It cannot
        // silently delete other project history when discussion text is removed.
        _db.TaskComments.Remove(comment);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task<TaskComment?> FindReplayAsync(Guid requestId, Guid userId) =>
        await _db.TaskComments.AsNoTracking().FirstOrDefaultAsync(comment =>
            comment.CreatedById == userId && comment.ClientRequestId == requestId);

    private async Task<TaskCommentDto?> ReplayAsync(TaskComment comment, Guid taskId,
        string text, Guid[] ids, Scope scope, Guid userId, Guid? replyToCommentId)
    {
        var existingIds = await _db.ProjectFiles.AsNoTracking()
            .Where(file => file.CommentId == comment.Id).Select(file => file.Id).ToListAsync();
        if (comment.TaskItemId != taskId || comment.Comment != text || comment.ReplyToCommentId != replyToCommentId
            || !existingIds.Order().SequenceEqual(ids.Order()))
            throw new ProjectFileException(409, "This request ID has already been used for a different comment. Refresh the discussion before posting again.");
        return await ReadAsync(comment.Id, scope, userId);
    }

    private async Task<TaskCommentDto?> ReadAsync(Guid id, Scope scope, Guid userId)
    {
        var comment = await _db.TaskComments.AsNoTracking().Include(item => item.CreatedBy)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (comment == null) return null;
        return (await MapAsync([comment], scope, userId))[0];
    }

    private async Task<List<TaskCommentDto>> MapAsync(List<TaskComment> comments, Scope scope, Guid userId)
    {
        var ids = comments.Select(comment => comment.Id).ToArray();
        var canModify = IsWritable(scope.Status);
        var owner = scope.OwnerId == userId;
        var files = await _db.ProjectFiles.AsNoTracking().Where(file => file.ProjectId == scope.ProjectId
            && file.CommentId != null && ids.Contains(file.CommentId.Value)
            && file.StoredFile.Status == StoredFileStatus.Ready)
            .OrderBy(file => file.CreatedAt)
            .Select(file => new ProjectFileDto(file.Id, file.ProjectId,
                file.StoredFile.OriginalFileName, file.StoredFile.ContentType, file.StoredFile.SizeBytes,
                file.UploadedByUserId, file.StoredFile.UploadedByUser.FullName,
                file.CreatedAt, "comment", file.TaskId,
                file.Task != null ? file.Task.Title : file.TaskTitle,
                canModify && (owner || file.UploadedByUserId == userId), file.CommentId))
            .ToListAsync();
        var grouped = files.GroupBy(file => file.CommentId!.Value)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<ProjectFileDto>)group.ToList());
        var parentIds = comments.Where(comment => comment.ReplyToCommentId.HasValue)
            .Select(comment => comment.ReplyToCommentId!.Value).Distinct().ToArray();
        var replyNames = await _db.TaskComments.AsNoTracking()
            .Where(parent => parentIds.Contains(parent.Id) && parent.TaskItem.ProjectId == scope.ProjectId)
            .Select(parent => new { parent.Id, Name = parent.CreatedBy.FullName })
            .ToDictionaryAsync(parent => parent.Id, parent => parent.Name);
        return comments.Select(comment => new TaskCommentDto(comment.Id, comment.CreatedAt,
            comment.UpdatedAt, comment.Comment, comment.CreatedById,
            comment.CreatedBy.FullName ?? "Allocatr user", comment.CreatedBy.AvatarUrl,
            comment.TaskItemId, grouped.GetValueOrDefault(comment.Id) ?? [], comment.ReplyToCommentId,
            comment.ReplyToCommentId.HasValue ? replyNames.GetValueOrDefault(comment.ReplyToCommentId.Value) : null)).ToList();
    }

    private async Task<Scope?> GetScopeAsync(Guid taskId, Guid userId, bool isAllocat)
    {
        var scope = await _db.TaskItems.AsNoTracking().Where(task => task.Id == taskId)
            .Select(task => new Scope(task.ProjectId, task.Project.UserId, task.Project.Status, task.AssignedToId, task.Title, task.Project.Title))
            .FirstOrDefaultAsync();
        if (scope == null || userId == Guid.Empty || (scope.OwnerId != userId
            && (!isAllocat || !await _projectAccess.IsAcceptedAllocatAsync(scope.ProjectId, userId)))) return null;
        return scope;
    }

    private static bool IsWritable(string status) => status != ProjectStatuses.Completed
        && status != ProjectStatuses.CompletionRequested;

    private static void ValidateComment(string text, bool hasAttachments)
    {
        if (text.Length == 0 && !hasAttachments) throw new ArgumentException("Write a comment or attach a file.");
        if (text.Length > 2000) throw new ArgumentException("Comment cannot exceed 2000 characters.");
    }
    private sealed record Scope(Guid ProjectId, Guid OwnerId, string Status, Guid? AssigneeId, string TaskTitle, string ProjectTitle);
}
