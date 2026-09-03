using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class ProjectService
{
    private readonly AllocatrDbContext _db;

    public ProjectService(AllocatrDbContext db)
    {
        _db = db;
    }

    /* =====================================================
       GET OWN PROJECTS
    ===================================================== */

    public async Task<List<ProjectDto>> GetProjectsByUserAsync(Guid userId)
    {
        var query = _db.Projects
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        return await ProjectDtoQuery(query)
            .ToListAsync();
    }

    /* =====================================================
       ACCESSIBLE PROJECTS
    ===================================================== */

    private IQueryable<Project> GetAccessibleProjectsQuery(
        Guid userId,
        bool isAllocat)
    {
        return _db.Projects
            .AsNoTracking()
            .Where(p =>
                p.UserId == userId ||
                (
                    isAllocat &&
                    p.AllocatAssignments.Any(pa =>
                        pa.AllocatProfile.AllocatrUserId == userId &&
                        pa.Status == ProjectAllocatStatus.Accepted &&
                        pa.RemovedAt == null
                    )
                )
            );
    }

    public async Task<List<ProjectDto>> GetAccessibleProjectsAsync(
        Guid userId,
        bool isAllocat)
    {
        var query = GetAccessibleProjectsQuery(userId, isAllocat)
            .OrderByDescending(p => p.CreatedAt);

        return await ProjectDtoQuery(query)
            .ToListAsync();
    }

    public async Task<ProjectDto?> GetAccessibleProjectByIdAsync(
        Guid projectId,
        Guid userId,
        bool isAllocat)
    {
        var query = GetAccessibleProjectsQuery(userId, isAllocat)
            .Where(p => p.Id == projectId);

        return await ProjectDtoQuery(query)
            .FirstOrDefaultAsync();
    }

    /* =====================================================
       UPDATE OWN PROJECT
    ===================================================== */

    public async Task<ProjectDto?> UpdateOwnedProjectAsync(
        Guid projectId,
        Guid currentUserId,
        UpdateProjectDto dto)
    {
        // Ownership is enforced as part of the database query.
        var project = await _db.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        if (project == null)
        {
            return null;
        }

        var title = dto.Title?.Trim() ?? string.Empty;
        var description = dto.Description?.Trim() ?? string.Empty;
        var priority = dto.Priority?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Project title is required."
            );
        }

        if (title.Length > 200)
        {
            throw new ArgumentException(
                "Project title cannot exceed 200 characters."
            );
        }

        if (description.Length > 2000)
        {
            throw new ArgumentException(
                "Project description cannot exceed 2,000 characters."
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

        if (
            dto.StartDate.HasValue &&
            dto.DueDate.HasValue &&
            dto.DueDate.Value < dto.StartDate.Value
        )
        {
            throw new ArgumentException(
                "The due date cannot be before the start date."
            );
        }

        project.Title = title;
        project.Description = description;
        project.StartDate = dto.StartDate;
        project.DueDate = dto.DueDate;
        project.Priority = priority;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Query a fresh DTO rather than serializing the tracked entity graph.
        var updatedProjectQuery = _db.Projects
            .AsNoTracking()
            .Where(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        return await ProjectDtoQuery(updatedProjectQuery)
            .FirstOrDefaultAsync();
    }

    /* =====================================================
       DTO PROJECTION
    ===================================================== */

    private static IQueryable<ProjectDto> ProjectDtoQuery(
        IQueryable<Project> query)
    {
        return query.Select(p =>
            new ProjectDto(
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
                    pa.Status == ProjectAllocatStatus.Accepted &&
                    pa.RemovedAt == null
                ),
                p.CreatedAt,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments
            )
        );
    }
}