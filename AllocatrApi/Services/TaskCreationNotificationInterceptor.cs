using System.Runtime.CompilerServices;
using AllocatrApi.Data;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace AllocatrApi.Services;

// Registered on AllocatrDbContext; leaves task creation/validation in the existing service.
public sealed class TaskCreationNotificationInterceptor : SaveChangesInterceptor
{
    private readonly ConditionalWeakTable<AllocatrDbContext, List<UserNotification>> _prepared =
        new();

    private TaskItem[] Prepare(AllocatrDbContext db)
    {
        // A failed save can be retried with changed task data. Replace only the rows
        // this interceptor queued, without touching other notification sources.
        if (_prepared.TryGetValue(db, out var previous))
            foreach (var item in previous)
                if (db.Entry(item).State == EntityState.Added)
                    db.Entry(item).State = EntityState.Detached;
        _prepared.Remove(db);
        _prepared.Add(db, new List<UserNotification>());
        return db
            .ChangeTracker.Entries<TaskItem>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToArray();
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default
    )
    {
        if (eventData.Context is not AllocatrDbContext db)
            return result;
        foreach (var task in Prepare(db))
        {
            var project =
                db.Projects.Local.FirstOrDefault(p => p.Id == task.ProjectId)
                ?? await db.Projects.AsNoTracking().SingleAsync(p => p.Id == task.ProjectId, ct);
            var members = await db
                .ProjectAllocats.AsNoTracking()
                .Where(a =>
                    a.ProjectId == task.ProjectId
                    && a.Status == ProjectAllocatStatus.Accepted
                    && a.RemovedAt == null
                    && a.AllocatProfile.AllocatrUser.IsAllocat
                )
                .Select(a => a.AllocatProfile.AllocatrUserId)
                .ToListAsync(ct);
            var actor = await db
                .Users.AsNoTracking()
                .Where(u => u.Id == task.CreatedByUserId)
                .Select(u => u.FullName)
                .SingleAsync(ct);
            Add(db, task, project.UserId, project.Title, members, actor);
        }
        return result;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        if (eventData.Context is not AllocatrDbContext db)
            return result;
        foreach (var task in Prepare(db))
        {
            var project =
                db.Projects.Local.FirstOrDefault(p => p.Id == task.ProjectId)
                ?? db.Projects.AsNoTracking().Single(p => p.Id == task.ProjectId);
            var members = db
                .ProjectAllocats.AsNoTracking()
                .Where(a =>
                    a.ProjectId == task.ProjectId
                    && a.Status == ProjectAllocatStatus.Accepted
                    && a.RemovedAt == null
                    && a.AllocatProfile.AllocatrUser.IsAllocat
                )
                .Select(a => a.AllocatProfile.AllocatrUserId)
                .ToList();
            var actor = db
                .Users.AsNoTracking()
                .Where(u => u.Id == task.CreatedByUserId)
                .Select(u => u.FullName)
                .Single();
            Add(db, task, project.UserId, project.Title, members, actor);
        }
        return result;
    }

    private void Add(
        AllocatrDbContext db,
        TaskItem task,
        Guid ownerId,
        string projectTitle,
        IEnumerable<Guid> acceptedMembers,
        string? actorName
    )
    {
        var members = acceptedMembers.Append(ownerId).ToHashSet();
        var recipients = new[] { ownerId, task.AssignedToId ?? Guid.Empty }
            .Where(id => id != Guid.Empty && id != task.CreatedByUserId && members.Contains(id))
            .Distinct();
        var name = string.IsNullOrWhiteSpace(actorName) ? "A project member" : actorName.Trim();
        foreach (var recipient in recipients)
        {
            if (
                db.Set<UserNotification>()
                    .Local.Any(n => n.EventId == task.Id && n.RecipientId == recipient)
            )
                continue;
            var text = $"{name} created the task {task.Title}.";
            var notification = new UserNotification
            {
                RecipientId = recipient,
                EventId = task.Id,
                Kind = "task-created",
                ActorId = task.CreatedByUserId,
                TaskId = task.Id,
                ProjectId = task.ProjectId,
                ProjectTitle = projectTitle.Length > 160 ? projectTitle[..160] : projectTitle,
                Text = text.Length > 500 ? text[..500] : text,
                Href = $"/projects/{task.ProjectId}?taskId={task.Id}",
            };
            db.Set<UserNotification>().Add(notification);
            _prepared.GetValue(db, _ => new List<UserNotification>()).Add(notification);
        }
    }
}
