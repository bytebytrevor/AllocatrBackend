using AllocatrApi.Data;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public sealed class CollaborationNotificationService(AllocatrDbContext db)
{
    public const int CommentGroupMinutes = 5;

    private async Task<HashSet<Guid>> CurrentMembersAsync(
        Guid projectId,
        Guid ownerId,
        CancellationToken ct
    )
    {
        var members = await db
            .ProjectAllocats.AsNoTracking()
            .Where(a =>
                a.ProjectId == projectId
                && a.Status == ProjectAllocatStatus.Accepted
                && a.RemovedAt == null
                && a.AllocatProfile.AllocatrUser.IsAllocat
            )
            .Select(a => a.AllocatProfile.AllocatrUserId)
            .ToListAsync(ct);
        return members.Append(ownerId).ToHashSet();
    }

    // Called after upload/access validation; the caller saves these rows with the Ready state.
    public async Task QueueFileAsync(
        Guid fileId,
        Guid projectId,
        string projectTitle,
        Guid ownerId,
        Guid actorId,
        string actorName,
        Guid? taskId,
        Guid? assigneeUserId,
        CancellationToken ct
    )
    {
        var members = await CurrentMembersAsync(projectId, ownerId, ct);
        IEnumerable<Guid> audience = taskId.HasValue
            ? new[] { ownerId, assigneeUserId ?? Guid.Empty }.Where(members.Contains)
            : members;
        var text = $"{Name(actorName)} added a file.";
        var href = taskId.HasValue
            ? TaskHref(projectId, taskId.Value)
            : $"/projects/{projectId}/files";
        var recipients = audience
            .Where(id => id != Guid.Empty && id != actorId)
            .Distinct()
            .ToArray();
        var alreadySent = await db.Set<UserNotification>()
            .AsNoTracking()
            .Where(n => n.EventId == fileId && recipients.Contains(n.RecipientId))
            .Select(n => n.RecipientId)
            .ToListAsync(ct);
        foreach (var recipient in recipients.Except(alreadySent))
            db.Set<UserNotification>()
                .Add(
                    new UserNotification
                    {
                        RecipientId = recipient,
                        EventId = fileId,
                        Kind = "file-added",
                        ActorId = actorId,
                        TaskId = taskId,
                        ProjectId = projectId,
                        ProjectTitle = Limit(projectTitle, 160),
                        Text = Limit(text, 500),
                        Href = href,
                    }
                );
    }

    // The caller's comment transaction covers the post, attachment binding and notifications.
    public async Task QueueCommentAsync(
        TaskComment comment,
        Guid projectId,
        string projectTitle,
        string taskTitle,
        Guid ownerId,
        Guid? assigneeUserId,
        bool repliesToOwner,
        string actorName,
        CancellationToken ct = default
    )
    {
        if (db.Database.CurrentTransaction == null)
            throw new InvalidOperationException(
                "Comment notifications require the comment transaction."
            );
        var members = await CurrentMembersAsync(projectId, ownerId, ct);
        var previousCommenters = await db
            .TaskComments.AsNoTracking()
            .Where(previous =>
                previous.TaskItemId == comment.TaskItemId
                && previous.TaskItem.ProjectId == projectId
                && previous.Id != comment.Id
            )
            .Select(previous => previous.CreatedById)
            .Distinct()
            .ToListAsync(ct);
        var recipients = previousCommenters
            .Append(ownerId)
            .Append(assigneeUserId ?? Guid.Empty)
            .Where(id => id != Guid.Empty && id != comment.CreatedById && members.Contains(id))
            .Distinct()
            .OrderBy(id => id)
            .ToArray();
        var now = DateTime.UtcNow;
        foreach (var recipient in recipients)
        {
            // Serializes first-group creation as well as increments across API instances.
            var key = $"allocatr:comment-notification:{recipient}";
            await db.Database.ExecuteSqlInterpolatedAsync(
                $"SELECT pg_advisory_xact_lock(hashtextextended({key}, 0))",
                ct
            );
            var latest = await db.Set<UserNotification>()
                .FromSqlInterpolated(
                    $"""
                    SELECT * FROM "UserNotifications"
                    WHERE "RecipientId" = {recipient}
                    ORDER BY "Id" DESC LIMIT 1 FOR UPDATE
                    """
                )
                .ToListAsync(ct);
            var group = latest.SingleOrDefault();
            if (
                group == null
                || group.Kind != "task-comment"
                || group.ActorId != comment.CreatedById
                || group.TaskId != comment.TaskItemId
                || group.ReadAt != null
                || (group.LastActivityAt ?? group.CreatedAt) < now.AddMinutes(-CommentGroupMinutes)
            )
            {
                group = new UserNotification
                {
                    RecipientId = recipient,
                    EventId = comment.Id,
                    Kind = "task-comment",
                    ProjectId = projectId,
                    ProjectTitle = Limit(projectTitle, 160),
                    ActorId = comment.CreatedById,
                    TaskId = comment.TaskItemId,
                };
                db.Set<UserNotification>().Add(group);
            }
            else
            {
                group.ItemCount++;
            }
            group.LastActivityAt = now;
            group.Text = Limit(
                $"{Name(actorName)} added {(group.ItemCount == 1 ? "a comment" : $"{group.ItemCount} comments")} on {taskTitle}.",
                500
            );
            group.Href = TaskHref(projectId, comment.TaskItemId) + $"&commentId={comment.Id}";
        }
    }

    private static string TaskHref(Guid projectId, Guid taskId) =>
        $"/projects/{projectId}?taskId={taskId}";

    private static string Name(string value) =>
        string.IsNullOrWhiteSpace(value) ? "A project member" : value.Trim();

    private static string Limit(string value, int max) => value.Length > max ? value[..max] : value;
}
