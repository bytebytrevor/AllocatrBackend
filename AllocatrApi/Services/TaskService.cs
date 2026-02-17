using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class TaskService
{
    private readonly AllocatrDbContext _db;

    public TaskService(AllocatrDbContext db)
    {
        _db = db;
    }

    /* --------------------------------------------------------
     * READ
     * -------------------------------------------------------- */

    // All tasks (admin / dashboard use)
    public async Task<List<TaskItem>> GetAllTasksAsync()
    {
        return await _db.TaskItems
            // .Include(t => t.AssignedTo)
            // .Include(t => t.CreatedByUser)
            // .Include(t => t.Comments)
            // .OrderBy(t => t.Order)
            .ToListAsync();

    }

    // All tasks for a specific project
    public async Task<List<TaskDto>> GetTasksByProjectAsync(Guid projectId)
    {
        // return await _db.TaskItems
        //     .Where(t => t.ProjectId == projectId)
        //     .OrderBy(t => t.Order)
        //     .Select(t => new TaskDto(
        //         t.Id,
        //         t.Title,
        //         t.Description,
        //         t.Status,
        //         t.Priority,
        //         t.DueDate,
        //         t.AssignedTo != null ? t.AssignedTo : null,
        //         t.CreatedByUser != null ? t.CreatedByUser : null
        //     ))
        //     .ToListAsync();

        return await _db.TaskItems
            .Where(t => t.ProjectId == projectId)
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


    // Single task
    public async Task<TaskItem?> GetTaskByIdAsync(Guid taskId)
    {
        return await _db.TaskItems
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedByUser)
            .Include(t => t.Comments)
                .ThenInclude(c => c.CreatedBy)
            .FirstOrDefaultAsync(t => t.Id == taskId);
    }

    /* --------------------------------------------------------
     * CREATE
     * -------------------------------------------------------- */

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        task.CreatedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        _db.TaskItems.Add(task);
        await _db.SaveChangesAsync();

        return task;
    }

    /* --------------------------------------------------------
     * UPDATE
     * -------------------------------------------------------- */

    public async Task<bool> UpdateTaskAsync(TaskItem updatedTask)
    {
        var existingTask = await _db.TaskItems
            .FirstOrDefaultAsync(t => t.Id == updatedTask.Id);

        if (existingTask == null)
            return false;

        existingTask.Title = updatedTask.Title;
        existingTask.Description = updatedTask.Description;
        existingTask.Status = updatedTask.Status;
        existingTask.Priority = updatedTask.Priority;
        existingTask.Order = updatedTask.Order;
        existingTask.AssignedToId = updatedTask.AssignedToId;
        existingTask.DueDate = updatedTask.DueDate;
        existingTask.CompletedAt = updatedTask.CompletedAt;
        existingTask.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    /* --------------------------------------------------------
     * DELETE
     * -------------------------------------------------------- */

    public async Task<bool> DeleteTaskAsync(Guid taskId)
    {
        var task = await _db.TaskItems.FindAsync(taskId);
        if (task == null)
            return false;

        _db.TaskItems.Remove(task);
        await _db.SaveChangesAsync();

        return true;
    }

    /* --------------------------------------------------------
     * STATUS HELPERS
     * -------------------------------------------------------- */

    public async Task<bool> MarkTaskCompletedAsync(Guid taskId)
    {
        var task = await _db.TaskItems.FindAsync(taskId);
        if (task == null)
            return false;

        task.Status = "done";
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignTaskAsync(Guid taskId, Guid? userId)
    {
        var task = await _db.TaskItems.FindAsync(taskId);
        if (task == null)
            return false;

        task.AssignedToId = userId.ToString();
        task.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }
}
