using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
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
    public async Task<List<ProjectDto>> GetProjectsByUserAsync(Guid userId)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
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

                // Argument 11
                p.AllocatAssignments.Any(pa =>
                    pa.Status == ProjectAllocatStatus.Accepted
                ),

                p.CreatedAt,
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
            .AsNoTracking()
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

                // Argument 11
                p.AllocatAssignments.Any(pa =>
                    pa.Status == ProjectAllocatStatus.Accepted
                ),

                p.CreatedAt,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments
            ))
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(Guid projectId)
    {
        return await _db.Projects
            .AsNoTracking()
            .Where(p => p.Id == projectId)
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

                p.AllocatAssignments.Any(pa =>
                    pa.Status == ProjectAllocatStatus.Accepted
                ),

                p.CreatedAt,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments
            ))
            .FirstOrDefaultAsync();
    }
}