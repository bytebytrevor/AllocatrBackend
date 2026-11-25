using System.Globalization;
using AllocatrApi.Data;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public enum NotificationEvent
{
    InvitationReceived,
    InvitationAccepted,
    InvitationDeclined,
    TaskAssigned,
    CompletionRequested,
    MoreWorkRequested,
    CompletionConfirmed,
    MessageReceived,
    ProjectCancelled,
    AllocatLeft,
}

public sealed record NotificationDto(
    string Id,
    string Kind,
    Guid ProjectId,
    string ProjectTitle,
    string Text,
    string Href,
    DateTime CreatedAt,
    DateTime? ReadAt
);

public sealed record NotificationPage(
    IReadOnlyList<NotificationDto> Items,
    string? NextCursor,
    int UnreadCount,
    string? LatestId
);

public sealed class NotificationService(AllocatrDbContext db)
{
    private IQueryable<UserNotification> For(Guid userId) =>
        db.Set<UserNotification>().Where(n => n.RecipientId == userId);

    // Called by an authorized domain service BEFORE its final SaveChangesAsync.
    // The caller supplies recipients from the current event's authorized audience,
    // and an event ID stable across retries (e.g. the saved message ID).
    // This method deliberately does not save unrelated tracked changes.
    public async Task QueueAsync(
        NotificationEvent kind,
        Guid eventId,
        Guid actorId,
        IEnumerable<Guid> recipientIds,
        Guid projectId,
        string projectTitle,
        string actorName,
        CancellationToken ct = default
    )
    {
        if (eventId == Guid.Empty || projectId == Guid.Empty || actorId == Guid.Empty)
            throw new ArgumentException("A persisted event, actor and project are required.");
        var recipients = recipientIds
            .Where(id => id != Guid.Empty && id != actorId)
            .Distinct()
            .ToArray();
        if (recipients.Length == 0)
            return;
        var existing = await db.Set<UserNotification>()
            .AsNoTracking()
            .Where(n => n.EventId == eventId && recipients.Contains(n.RecipientId))
            .Select(n => n.RecipientId)
            .ToListAsync(ct);
        var queued = db.Set<UserNotification>()
            .Local.Where(n => n.EventId == eventId)
            .Select(n => n.RecipientId);
        var skip = existing.Concat(queued).ToHashSet();
        var (key, action) = kind switch
        {
            NotificationEvent.InvitationReceived => (
                "invitation-received",
                "invited you to a project."
            ),
            NotificationEvent.InvitationAccepted => (
                "invitation-accepted",
                "accepted your project invitation."
            ),
            NotificationEvent.InvitationDeclined => (
                "invitation-declined",
                "declined your project invitation."
            ),
            NotificationEvent.TaskAssigned => ("task-assigned", "assigned you a task."),
            NotificationEvent.CompletionRequested => (
                "completion-requested",
                "requested project completion."
            ),
            NotificationEvent.MoreWorkRequested => (
                "more-work-requested",
                "requested more work on this project."
            ),
            NotificationEvent.CompletionConfirmed => (
                "completion-confirmed",
                "confirmed project completion."
            ),
            NotificationEvent.MessageReceived => ("message-received", "sent a new message."),
            NotificationEvent.ProjectCancelled => ("project-cancelled", "cancelled this project."),
            NotificationEvent.AllocatLeft => (
                "allocat-left",
                "left this project. Their unfinished tasks are now unassigned."
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
        var name = string.IsNullOrWhiteSpace(actorName) ? "A project member" : actorName.Trim();
        var text = $"{name} {action}";
        var href =
            kind == NotificationEvent.InvitationReceived
                ? "/notifications"
                : $"/projects/{projectId}"
                    + (kind == NotificationEvent.MessageReceived ? "/messaging" : "");
        foreach (var recipient in recipients.Where(id => !skip.Contains(id)))
            db.Set<UserNotification>()
                .Add(
                    new UserNotification
                    {
                        RecipientId = recipient,
                        EventId = eventId,
                        Kind = key,
                        ProjectId = projectId,
                        ProjectTitle =
                            projectTitle.Length > 160 ? projectTitle[..160] : projectTitle,
                        Text = text.Length > 500 ? text[..500] : text,
                        Href = href,
                    }
                );
    }

    public Task<int> UnreadCountAsync(Guid userId, CancellationToken ct) =>
        For(userId).CountAsync(n => n.ReadAt == null, ct);

    public async Task<NotificationPage> ListAsync(
        Guid userId,
        long? before,
        int take,
        CancellationToken ct
    )
    {
        var query = For(userId).AsNoTracking();
        if (before.HasValue)
            query = query.Where(n => n.Id < before.Value);
        var rows = await query
            .OrderByDescending(n => n.Id)
            .Take(Math.Clamp(take, 1, 50) + 1)
            .ToListAsync(ct);
        var hasMore = rows.Count > Math.Clamp(take, 1, 50);
        if (hasMore)
            rows.RemoveAt(rows.Count - 1);
        var items = rows.Select(n => new NotificationDto(
                n.Id.ToString(CultureInfo.InvariantCulture),
                n.Kind,
                n.ProjectId,
                n.ProjectTitle,
                n.Text,
                n.Href,
                n.LastActivityAt ?? n.CreatedAt,
                n.ReadAt
            ))
            .ToArray();
        return new NotificationPage(
            items,
            hasMore ? items[^1].Id : null,
            await UnreadCountAsync(userId, ct),
            items.FirstOrDefault()?.Id
        );
    }

    public Task<int> MarkReadAsync(Guid userId, long id, CancellationToken ct) =>
        For(userId)
            .Where(n => n.Id == id && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, (DateTime?)DateTime.UtcNow), ct);

    // Bound to the latest ID the user actually loaded. New arrivals remain unread.
    public Task<int> MarkAllReadAsync(Guid userId, long throughId, CancellationToken ct) =>
        For(userId)
            .Where(n => n.Id <= throughId && n.ReadAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.ReadAt, (DateTime?)DateTime.UtcNow), ct);
}
