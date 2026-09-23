using System.Linq;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Constants;

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
        var project = await _db.Projects
            .Include(p => p.ProjectSkills)
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        if (project == null)
        {
            return null;
        }

        if (
            project.Status == ProjectStatuses.CompletionRequested ||
            project.Status == ProjectStatuses.Completed
        )
        {
            throw new InvalidOperationException(
                "This project cannot be edited in its current state."
            );
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


        // Validate updated skills
        List<Guid>? skillIds = null;

        if (dto.SkillIds != null)
        {
            skillIds = dto.SkillIds
                .Distinct()
                .ToList();

            if (skillIds.Count == 0)
            {
                throw new ArgumentException(
                    "Select at least one skill for this project."
                );
            }

            if (skillIds.Count > 15)
            {
                throw new ArgumentException(
                    "A project cannot have more than 15 skills."
                );
            }

            var validSkillIds = await _db.Skills
                .Where(s => skillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            if (validSkillIds.Count != skillIds.Count)
            {
                throw new ArgumentException(
                    "One or more selected skills are invalid."
                );
            }
        }


        // Update project

        project.Title = title;
        project.Description = description;
        project.StartDate = dto.StartDate;
        project.DueDate = dto.DueDate;
        project.Priority = priority;
        project.UpdatedAt = DateTime.UtcNow;


        // Update project skills

        if (skillIds != null)
        {
            _db.ProjectSkills.RemoveRange(
                project.ProjectSkills
            );

            project.ProjectSkills = skillIds
                .Select(skillId => new ProjectSkill
                {
                    ProjectId = project.Id,
                    SkillId = skillId
                })
                .ToList();
        }

        await _db.SaveChangesAsync();


        // Return fresh dto

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
   COMPLETE OWN PROJECT
    ===================================================== */

    public async Task<ProjectDto?> CompleteOwnedProjectAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        if (project == null)
        {
            return null;
        }

        /*
        * Treat completion as idempotent.
        *
        * If the client submits the request twice,
        * simply return the already completed project.
        */
        if (
            project.Status ==
            ProjectStatuses.Completed
        )
        {
            return await ProjectDtoQuery(
                    _db.Projects
                        .AsNoTracking()
                        .Where(p =>
                            p.Id == projectId
                        )
                )
                .FirstOrDefaultAsync();
        }

        /*
        * A pending project has not started yet.
        *
        * Projects become active when the first
        * invited Allocat accepts.
        */
        if (
            project.Status !=
            ProjectStatuses.Active
        )
        {
            throw new InvalidOperationException(
                "Only active projects can be completed."
            );
        }

        /*
        * Defensive validation.
        *
        * An active project should already have at
        * least one accepted Allocat, but we protect
        * against inconsistent data.
        */
        var hasAcceptedAllocat =
            await _db.ProjectAllocats
                .AsNoTracking()
                .AnyAsync(pa =>
                    pa.ProjectId == projectId &&
                    pa.Status ==
                        ProjectAllocatStatus.Accepted &&
                    pa.RemovedAt == null
                );

        if (!hasAcceptedAllocat)
        {
            throw new InvalidOperationException(
                "A project must have at least one accepted Allocat before it can be completed."
            );
        }

        /*
        * If tasks exist, every task must be complete.
        *
        * Projects with no tasks can still be confirmed
        * complete by the client.
        */
        var hasIncompleteTasks =
            await _db.TaskItems
                .AsNoTracking()
                .AnyAsync(t =>
                    t.ProjectId == projectId &&
                    t.Status != "complete"
                );

        if (hasIncompleteTasks)
        {
            throw new InvalidOperationException(
                "All project tasks must be completed before the project can be confirmed complete."
            );
        }

        var now = DateTime.UtcNow;

        project.Status =
            ProjectStatuses.Completed;

        project.Progress = 100;

        project.CompletedAt = now;

        project.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return await ProjectDtoQuery(
                _db.Projects
                    .AsNoTracking()
                    .Where(p =>
                        p.Id == projectId
                    )
            )
            .FirstOrDefaultAsync();
    }

    /* =====================================================
    REQUEST PROJECT COMPLETION
    ===================================================== */

    public async Task<ProjectDto?> RequestCompletionAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            return null;
        }

        var isAcceptedAllocat = await _db.ProjectAllocats
            .AsNoTracking()
            .AnyAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == currentUserId &&
                pa.Status == ProjectAllocatStatus.Accepted &&
                pa.RemovedAt == null
            );

        if (!isAcceptedAllocat)
        {
            throw new UnauthorizedAccessException(
                "Only an accepted Allocat can request project completion."
            );
        }

        if (project.Status == ProjectStatuses.CompletionRequested)
        {
            return await GetProjectDtoAsync(projectId);
        }

        if (project.Status == ProjectStatuses.Completed)
        {
            throw new InvalidOperationException(
                "This project has already been completed."
            );
        }

        if (project.Status != ProjectStatuses.Active)
        {
            throw new InvalidOperationException(
                "Only active projects can be marked ready for completion."
            );
        }

        var now = DateTime.UtcNow;

        foreach (var task in project.Tasks.Where(t => t.Status != "complete"))
        {
            task.Status = "complete";
            task.CompletedAt = now;
            task.UpdatedAt = now;
        }

        project.Status = ProjectStatuses.CompletionRequested;
        project.Progress = 100;
        project.CompletionRequestedAt = now;
        project.CompletionRequestedByAllocatId = currentUserId;
        project.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return await GetProjectDtoAsync(projectId);
    }

    /* =====================================================
    CONFIRM PROJECT COMPLETION
    ===================================================== */

    public async Task<ProjectDto?> ConfirmCompletionAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        if (project == null)
        {
            return null;
        }

        if (project.Status == ProjectStatuses.Completed)
        {
            return await GetProjectDtoAsync(projectId);
        }

        if (project.Status != ProjectStatuses.CompletionRequested)
        {
            throw new InvalidOperationException(
                "This project is not awaiting completion confirmation."
            );
        }

        var now = DateTime.UtcNow;

        project.Status = ProjectStatuses.Completed;
        project.Progress = 100;
        project.CompletedAt = now;
        project.UpdatedAt = now;

        await _db.SaveChangesAsync();

        return await GetProjectDtoAsync(projectId);
    }

    /* =====================================================
    RETURN PROJECT TO ACTIVE
    ===================================================== */

    public async Task<ProjectDto?> NeedsMoreWorkAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId &&
                p.UserId == currentUserId
            );

        if (project == null)
        {
            return null;
        }

        if (project.Status != ProjectStatuses.CompletionRequested)
        {
            throw new InvalidOperationException(
                "This project is not awaiting completion confirmation."
            );
        }

        project.Status = ProjectStatuses.Active;
        project.CompletionRequestedAt = null;
        project.CompletionRequestedByAllocatId = null;
        project.CompletedAt = null;
        project.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetProjectDtoAsync(projectId);
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
                p.CompletionRequestedAt,
                p.CompletionRequestedByAllocatId,
                p.CompletedAt,
                p.StartDate,
                p.DueDate,
                p.AllocatAssignments,

                p.ProjectSkills
                    .Select(ps => new ProjectSkillDto(
                        ps.Skill.Id,
                        ps.Skill.Name,
                        ps.Skill.SkillCategoryId,
                        ps.Skill.SkillCategory.Name
                    ))
                    .ToList()
            )
        );
    }

    private async Task<ProjectDto?> GetProjectDtoAsync(Guid projectId)
    {
        return await ProjectDtoQuery(
            _db.Projects
                .AsNoTracking()
                .Where(p => p.Id == projectId)
        )
        .FirstOrDefaultAsync();
    }
}