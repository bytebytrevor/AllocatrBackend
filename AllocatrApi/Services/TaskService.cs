using System;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class TaskService
{
    private readonly AllocatrDbContext _db;

    public TaskService(AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<List<TaskDto>> GetAllTasksAsync()
    {
        return await _db.Tasks
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.Status,
                t.Priority,
                t.Order,
                t.AssignedToId,
                t.UpdatedAt,
                t.CreatedAt,
                t.DueDate,
                t.CompletedAt,
                t.Comments,
                t.CreatedByUserId,
                t.ProjectId
            ))
            .ToListAsync();
    }
}
