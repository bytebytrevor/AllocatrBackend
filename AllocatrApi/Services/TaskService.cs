using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class TaskService
{
    private readonly AllocatrDbContext _db;
    private readonly ProjectAccessService _projectAccess;
    private readonly ProjectService _projectService;

    public TaskService(
        AllocatrDbContext db,
        ProjectAccessService projectAccess,
        ProjectService projectService)
    {
        _db = db;
        _projectAccess = projectAccess;
        _projectService = projectService;
    }

    public async Task<List<TaskDto>?> GetTasksByProjectIdAsync(
        Guid projectId,
        Guid userId,
        bool isAllocat)
    {
        var canView = await _projectAccess.CanViewProjectAsync(
            projectId,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return null;
        }

        return await _db.TaskItems
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Order)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate
            ))
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return null;
        }

        var canView = await _projectAccess.CanViewProjectAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (!canView)
        {
            return null;
        }

        return await _db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.DueDate
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<TaskDto?> CreateTaskAsync(
        Guid projectId,
        Guid userId,
        bool isAllocat,
        CreateTaskDto dto)
    {
        if (!isAllocat)
        {
            return null;
        }

        var canExecute = await _projectAccess.IsAcceptedAllocatAsync(
            projectId,
            userId
        );

        if (!canExecute)
        {
            return null;
        }

        var title = dto.Title?.Trim() ?? string.Empty;
        var description = dto.Description?.Trim() ?? string.Empty;
        var priority = dto.Priority?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.");
        }

        if (title.Length > 200)
        {
            throw new ArgumentException(
                "Task title cannot exceed 200 characters."
            );
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException(
                "Task description cannot exceed 2,000 characters."
            );
        }

        if (
            priority != "standard" &&
            priority != "high" &&
            priority != "urgent"
        )
        {
            throw new ArgumentException(
                "Priority must be standard, high or urgent."
            );
        }

        if (dto.AssignedToId.HasValue)
        {
            var validAssignee =
                await _projectAccess.IsAcceptedAllocatAsync(
                    projectId,
                    dto.AssignedToId.Value
                );

            if (!validAssignee)
            {
                throw new ArgumentException(
                    "The selected user is not an accepted Allocat on this project."
                );
            }
        }

        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = title,
            Description = description,
            Priority = priority,
            Status = "pending",
            DueDate = dto.DueDate,
            AssignedToId = dto.AssignedToId,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        await RecalculateProjectProgressAsync(projectId);

        return ToTaskDto(task);
    }

    public async Task<UpdateTaskStatusResultDto?> UpdateTaskStatusAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat,
        string status)
    {
        var normalizedStatus =
            status?.Trim().ToLowerInvariant() ?? string.Empty;

        if (
            normalizedStatus != "pending" &&
            normalizedStatus != "active" &&
            normalizedStatus != "complete" &&
            normalizedStatus != "overdue"
        )
        {
            throw new ArgumentException("Invalid task status.");
        }

        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue || !isAllocat)
        {
            return null;
        }

        var canExecute = await _projectAccess.IsAcceptedAllocatAsync(
            projectId.Value,
            userId
        );

        if (!canExecute)
        {
            return null;
        }

        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.ProjectId == projectId.Value
            );

        if (task == null)
        {
            return null;
        }

        task.Status = normalizedStatus;
        task.CompletedAt =
            normalizedStatus == "complete"
                ? DateTime.UtcNow
                : null;

        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await RecalculateProjectProgressAsync(projectId.Value);

        var project = await _projectService.GetAccessibleProjectByIdAsync(
            projectId.Value,
            userId,
            isAllocat
        );

        if (project == null)
        {
            return null;
        }

        return new UpdateTaskStatusResultDto(
            ToTaskDto(task),
            project
        );
    }

    public async Task<bool> DeleteTaskAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat)
    {
        if (!isAllocat)
        {
            return false;
        }

        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue)
        {
            return false;
        }

        var canExecute = await _projectAccess.IsAcceptedAllocatAsync(
            projectId.Value,
            userId
        );

        if (!canExecute)
        {
            return false;
        }

        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t =>
                t.Id == taskId &&
                t.ProjectId == projectId.Value
            );

        if (task == null)
        {
            return false;
        }

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();

        await RecalculateProjectProgressAsync(projectId.Value);

        return true;
    }

    public async Task<bool> MarkTaskCompletedAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat)
    {
        var result = await UpdateTaskStatusAsync(
            taskId,
            userId,
            isAllocat,
            "complete"
        );

        return result != null;
    }

    public async Task<bool> AssignTaskAsync(
        Guid taskId,
        Guid userId,
        bool isAllocat,
        Guid? assignedToId)
    {
        var projectId = await GetTaskProjectIdAsync(taskId);

        if (!projectId.HasValue || !isAllocat)
        {
            return false;
        }

        var canExecute = await _projectAccess.IsAcceptedAllocatAsync(
            projectId.Value,
            userId
        );

        if (!canExecute)
        {
            return false;
        }

        if (assignedToId.HasValue)
        {
            var validAssignee =
                await _projectAccess.IsAcceptedAllocatAsync(
                    projectId.Value,
                    assignedToId.Value
                );

            if (!validAssignee)
            {
                throw new ArgumentException(
                    "The selected user is not an accepted Allocat on this project."
                );
            }
        }

        var task = await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return false;
        }

        task.AssignedToId = assignedToId;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    private async Task<Guid?> GetTaskProjectIdAsync(Guid taskId)
    {
        return await _db.TaskItems
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => (Guid?)t.ProjectId)
            .FirstOrDefaultAsync();
    }

    private async Task RecalculateProjectProgressAsync(
        Guid projectId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return;
        }

        var totalTasks = await _db.TaskItems
            .CountAsync(t => t.ProjectId == projectId);

        var completedTasks = await _db.TaskItems
            .CountAsync(t =>
                t.ProjectId == projectId &&
                t.Status == "complete"
            );

        project.Progress =
            totalTasks == 0
                ? 0
                : (int)Math.Round(
                    completedTasks * 100.0 / totalTasks
                );

        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static TaskDto ToTaskDto(TaskItem task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.Priority,
            task.DueDate
        );
    }
}