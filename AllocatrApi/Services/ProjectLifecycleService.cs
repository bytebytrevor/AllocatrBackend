using AllocatrApi.Constants;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public sealed class ProjectLifecycleService(AllocatrDbContext db, NotificationService notifications)
{
    public async Task<ProjectLifecycleResult> CancelAsync(
        Guid projectId,
        Guid ownerId,
        string reason,
        CancellationToken ct = default
    )
    {
        reason = ValidateReason(reason);
        await using var scope = await ProjectMutationScope.BeginAsync(db, projectId, ct);
        var project =
            await db.Projects.SingleOrDefaultAsync(
                p => p.Id == projectId && p.UserId == ownerId,
                ct
            ) ?? throw new KeyNotFoundException("Project not found.");
        if (project.Status == ProjectStatuses.Cancelled)
        {
            var previous = await db.Set<ProjectLifecycleEvent>()
                .AsNoTracking()
                .Where(e => e.ProjectId == projectId && e.Kind == "project-cancelled")
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(ct);
            return new(projectId, previous, false);
        }
        if (project.Status == ProjectStatuses.CompletionRequested)
            throw new InvalidOperationException(
                "Choose Needs more work before cancelling this project."
            );
        if (project.Status != ProjectStatuses.Pending && project.Status != ProjectStatuses.Active)
            throw new InvalidOperationException(
                "Only pending or active projects can be cancelled."
            );

        var assignments = await db
            .ProjectAllocats.Where(p => p.ProjectId == projectId && p.RemovedAt == null)
            .ToListAsync(ct);
        var recipients = await db
            .ProjectAllocats.AsNoTracking()
            .Where(p =>
                p.ProjectId == projectId
                && p.Status == ProjectAllocatStatus.Accepted
                && p.RemovedAt == null
            )
            .Select(p => p.AllocatProfile.AllocatrUserId)
            .Where(id => id != ownerId)
            .Distinct()
            .ToArrayAsync(ct);
        var now = DateTime.UtcNow;
        project.Status = ProjectStatuses.Cancelled;
        project.CancelledAt = now;
        project.CancelledByUserId = ownerId;
        project.CancellationReason = reason;
        project.UpdatedAt = now;
        // Accepted relationships and every task/contribution stay intact for history/discussion.
        foreach (var invitation in assignments.Where(p => p.Status == ProjectAllocatStatus.Invited))
        {
            invitation.Status = ProjectAllocatStatus.Withdrawn;
            invitation.RemovedAt = now;
        }
        var savedEvent = await RecordAsync(
            project,
            ownerId,
            "project-cancelled",
            reason,
            recipients,
            now,
            ct
        );
        await notifications.QueueAsync(
            NotificationEvent.ProjectCancelled,
            savedEvent.Id,
            ownerId,
            recipients,
            projectId,
            project.Title,
            savedEvent.ActorName,
            ct
        );
        await db.SaveChangesAsync(ct);
        await scope.CommitAsync(ct);
        return new(projectId, savedEvent.Id, true);
    }

    public async Task<ProjectLifecycleResult> LeaveAsync(
        Guid projectId,
        Guid userId,
        string reason,
        CancellationToken ct = default
    )
    {
        reason = ValidateReason(reason);
        await using var scope = await ProjectMutationScope.BeginAsync(db, projectId, ct);
        var relationship =
            await db
                .ProjectAllocats.Include(p => p.Project)
                .SingleOrDefaultAsync(
                    p => p.ProjectId == projectId && p.AllocatProfile.AllocatrUserId == userId,
                    ct
                )
            ?? throw new KeyNotFoundException("Project membership not found.");
        var project = relationship.Project;
        if (project.UserId == userId)
            throw new InvalidOperationException(
                "The project owner cannot leave their own project."
            );
        if (relationship.Status == ProjectAllocatStatus.Left)
        {
            var previous = await db.Set<ProjectLifecycleEvent>()
                .AsNoTracking()
                .Where(e =>
                    e.ProjectId == projectId && e.ActorId == userId && e.Kind == "allocat-left"
                )
                .OrderByDescending(e => e.CreatedAt)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(ct);
            return new(projectId, previous, false);
        }
        if (relationship.Status != ProjectAllocatStatus.Accepted || relationship.RemovedAt != null)
            throw new InvalidOperationException(
                "Only a current accepted Allocat can leave this project."
            );
        if (project.Status != ProjectStatuses.Pending && project.Status != ProjectStatuses.Active)
            throw new InvalidOperationException(
                "You can leave only while the project is pending or active. Resolve a pending completion request with the client first."
            );

        var now = DateTime.UtcNow;
        relationship.Status = ProjectAllocatStatus.Left;
        relationship.RemovedAt = now;
        relationship.LeftAt = now;
        relationship.LeaveReason = reason;
        // Completed attribution and all discussion/file authorship remain unchanged.
        var unfinished = await db
            .TaskItems.Where(t =>
                t.ProjectId == projectId && t.AssignedToId == userId && t.Status != "complete"
            )
            .ToListAsync(ct);
        foreach (var task in unfinished)
        {
            task.AssignedToId = null;
            task.UpdatedAt = now;
        }
        project.UpdatedAt = now;
        var savedEvent = await RecordAsync(
            project,
            userId,
            "allocat-left",
            reason,
            [project.UserId],
            now,
            ct
        );
        await notifications.QueueAsync(
            NotificationEvent.AllocatLeft,
            savedEvent.Id,
            userId,
            [project.UserId],
            projectId,
            project.Title,
            savedEvent.ActorName,
            ct
        );
        await db.SaveChangesAsync(ct);
        await scope.CommitAsync(ct);
        return new(projectId, savedEvent.Id, true);
    }

    private async Task<ProjectLifecycleEvent> RecordAsync(
        Project project,
        Guid actorId,
        string kind,
        string reason,
        Guid[] recipients,
        DateTime now,
        CancellationToken ct
    )
    {
        var name = await db
            .Users.AsNoTracking()
            .Where(u => u.Id == actorId)
            .Select(u => u.FullName)
            .SingleAsync(ct);
        var item = new ProjectLifecycleEvent
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ActorId = actorId,
            Kind = kind,
            Reason = reason,
            RecipientIds = recipients,
            CreatedAt = now,
            ProjectTitle = project.Title,
            ProjectCode = project.ProjectCode,
            ActorName = string.IsNullOrWhiteSpace(name) ? "A project member" : name.Trim(),
        };
        db.Set<ProjectLifecycleEvent>().Add(item);
        return item;
    }

    private static string ValidateReason(string? reason)
    {
        var text = reason?.Trim() ?? "";
        if (text.Length == 0 || text.Length > 1000)
            throw new ArgumentException("Provide a reason of 1 to 1,000 characters.");
        return text;
    }
}
