using System;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class ProjectService
{
    private readonly AllocatrDbContext _db;

    public ProjectService(AllocatrDbContext db)
    {
        _db = db;
    }

    // Get all projects for logged in user
    public async Task<List<ProjectDto>> GetProjectsByUserAsync(string userId)
    {
        return await _db.Projects
            .Where(p => p.UserId == userId)
            .Select(p => new ProjectDto
            (
                p.Id,
                p.ProjectCode,
                p.Title,
                p.Description,
                p.Category,
                p.Status,
                p.Progress,
                p.Priority,
                p.Budget,
                p.Currency,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments
            ))
            .ToListAsync();
    }

    // Get all projects
    public async Task<List<ProjectDto>> GetAllProjectsAsync()
    {
        return await _db.Projects
            .Select(p => new ProjectDto(
                p.Id,
                p.ProjectCode,
                p.Title,
                p.Description,
                p.Category,
                p.Status,
                p.Progress,
                p.Priority,
                p.Budget,
                p.Currency,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments
            ))
            .ToListAsync();
    }
}

