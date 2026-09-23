using AllocatrApi.Constants;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class ProjectAllocatService
{
    private readonly AllocatrDbContext _db;

    public ProjectAllocatService(AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<ProjectAllocatDto> InviteAllocatAsync(
        Guid projectId,
        Guid allocatProfileId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId);

        if (project == null)
        {
            throw new KeyNotFoundException(
                "Project not found."
            );
        }

        if (project.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to invite Allocats to this project."
            );
        }

        if (
            project.Status == ProjectStatuses.CompletionRequested ||
            project.Status == ProjectStatuses.Completed
        )
        {
            throw new InvalidOperationException(
                "Allocats cannot be invited while this project is awaiting completion confirmation or has been completed."
            );
        }

        var allocatExists = await _db.AllocatProfiles
            .AnyAsync(a =>
                a.AllocatrUserId == allocatProfileId
            );

        if (!allocatExists)
        {
            throw new KeyNotFoundException(
                "Allocat not found."
            );
        }

        var existingRelationship = await _db.ProjectAllocats
            .FirstOrDefaultAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == allocatProfileId
            );

        if (existingRelationship != null)
        {
            switch (existingRelationship.Status)
            {
                case ProjectAllocatStatus.Invited:
                    throw new InvalidOperationException(
                        "This Allocat has already been invited."
                    );

                case ProjectAllocatStatus.Accepted:
                    throw new InvalidOperationException(
                        "This Allocat is already part of the project."
                    );

                case ProjectAllocatStatus.Declined:
                case ProjectAllocatStatus.Removed:
                    existingRelationship.Status =
                        ProjectAllocatStatus.Invited;

                    existingRelationship.InvitedAt =
                        DateTime.UtcNow;

                    existingRelationship.RespondedAt = null;
                    existingRelationship.RemovedAt = null;

                    await _db.SaveChangesAsync();

                    return ToDto(existingRelationship);
            }
        }

        var projectAllocat = new ProjectAllocat
        {
            ProjectId = projectId,
            AllocatProfileId = allocatProfileId,
            Status = ProjectAllocatStatus.Invited,
            InvitedAt = DateTime.UtcNow,
            RespondedAt = null,
            RemovedAt = null
        };

        _db.ProjectAllocats.Add(projectAllocat);

        await _db.SaveChangesAsync();

        return ToDto(projectAllocat);
    }

    public async Task<ProjectAllocatDto> AcceptInviteAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var allocatProfile = await _db.AllocatProfiles
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == currentUserId
            );

        if (allocatProfile == null)
        {
            throw new UnauthorizedAccessException(
                "You do not have an Allocat profile."
            );
        }

        var relationship = await _db.ProjectAllocats
            .Include(pa => pa.Project)
            .FirstOrDefaultAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == allocatProfile.AllocatrUserId
            );

        if (relationship == null)
        {
            throw new KeyNotFoundException(
                "Invitation not found."
            );
        }

        if (
            relationship.Status !=
            ProjectAllocatStatus.Invited
        )
        {
            throw new InvalidOperationException(
                "This invitation is no longer pending."
            );
        }

        if (
            relationship.Project.Status ==
                ProjectStatuses.CompletionRequested ||
            relationship.Project.Status ==
                ProjectStatuses.Completed
        )
        {
            throw new InvalidOperationException(
                "Invitations cannot be accepted while this project is awaiting completion confirmation or has been completed."
            );
        }

        var now = DateTime.UtcNow;

        if (
            relationship.Project.Status ==
            ProjectStatuses.Pending
        )
        {
            relationship.Project.Status =
                ProjectStatuses.Active;

            relationship.Project.UpdatedAt = now;
        }

        relationship.Status =
            ProjectAllocatStatus.Accepted;

        relationship.RespondedAt = now;

        await _db.SaveChangesAsync();

        return ToDto(relationship);
    }

    public async Task<ProjectAllocatDto> DeclineInviteAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var allocatProfile = await _db.AllocatProfiles
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == currentUserId
            );

        if (allocatProfile == null)
        {
            throw new UnauthorizedAccessException(
                "You do not have an Allocat profile."
            );
        }

        var relationship = await _db.ProjectAllocats
            .FirstOrDefaultAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == allocatProfile.AllocatrUserId
            );

        if (relationship == null)
        {
            throw new KeyNotFoundException(
                "Invitation not found."
            );
        }

        if (
            relationship.Status !=
            ProjectAllocatStatus.Invited
        )
        {
            throw new InvalidOperationException(
                "This invitation is no longer pending."
            );
        }

        relationship.Status =
            ProjectAllocatStatus.Declined;

        relationship.RespondedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToDto(relationship);
    }

    public async Task<ProjectAllocatDto> RemoveAllocatAsync(
        Guid projectId,
        Guid allocatProfileId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p =>
                p.Id == projectId
            );

        if (project == null)
        {
            throw new KeyNotFoundException(
                "Project not found."
            );
        }

        if (project.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You are not allowed to remove Allocats from this project."
            );
        }

        if (
            project.Status == ProjectStatuses.CompletionRequested ||
            project.Status == ProjectStatuses.Completed
        )
        {
            throw new InvalidOperationException(
                "Allocats cannot be removed while this project is awaiting completion confirmation or has been completed."
            );
        }

        var relationship = await _db.ProjectAllocats
            .FirstOrDefaultAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == allocatProfileId
            );

        if (relationship == null)
        {
            throw new KeyNotFoundException(
                "Allocat is not attached to this project."
            );
        }

        if (
            relationship.Status !=
            ProjectAllocatStatus.Accepted
        )
        {
            throw new InvalidOperationException(
                "Only accepted Allocats can be removed."
            );
        }

        relationship.Status =
            ProjectAllocatStatus.Removed;

        relationship.RemovedAt =
            DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return ToDto(relationship);
    }

    public async Task<ProjectAllocatDto?> GetProjectAllocatAsync(
        Guid projectId,
        Guid allocatProfileId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == projectId
            );

        if (project == null)
        {
            return null;
        }

        var canView =
            project.UserId == currentUserId ||
            allocatProfileId == currentUserId;

        if (!canView)
        {
            throw new UnauthorizedAccessException();
        }

        var projectAllocat = await _db.ProjectAllocats
            .AsNoTracking()
            .FirstOrDefaultAsync(pa =>
                pa.ProjectId == projectId &&
                pa.AllocatProfileId == allocatProfileId
            );

        return projectAllocat == null
            ? null
            : ToDto(projectAllocat);
    }

    public async Task<List<ProjectAllocatDto>> GetProjectAllocatsAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == projectId
            );

        if (project == null)
        {
            throw new KeyNotFoundException(
                "Project not found."
            );
        }

        if (project.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view this project's Allocats."
            );
        }

        return await _db.ProjectAllocats
            .AsNoTracking()
            .Where(pa =>
                pa.ProjectId == projectId
            )
            .Select(pa => new ProjectAllocatDto(
                pa.ProjectId,
                pa.AllocatProfileId,
                pa.Status,
                pa.InvitedAt,
                pa.RespondedAt,
                pa.RemovedAt
            ))
            .ToListAsync();
    }

    public async Task<List<ProjectAllocatMemberDto>> GetProjectMembersAsync(
        Guid projectId,
        Guid currentUserId)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p =>
                p.Id == projectId
            );

        if (project == null)
        {
            throw new KeyNotFoundException(
                "Project not found."
            );
        }

        if (project.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view this project's members."
            );
        }

        return await _db.ProjectAllocats
            .AsNoTracking()
            .Where(pa =>
                pa.ProjectId == projectId &&
                (
                    pa.Status == ProjectAllocatStatus.Invited ||
                    pa.Status == ProjectAllocatStatus.Accepted
                )
            )
            .Select(pa => new ProjectAllocatMemberDto(
                pa.AllocatProfileId,
                pa.AllocatProfile.AllocatrUser.FullName,
                pa.AllocatProfile.AllocatrUser.AvatarUrl,
                pa.Status,
                pa.InvitedAt,
                pa.RespondedAt
            ))
            .ToListAsync();
    }

    public async Task<List<AllocatWorkProjectDto>> GetMyWorkProjectsAsync(
        Guid currentUserId)
    {
        var allocatExists = await _db.AllocatProfiles
            .AsNoTracking()
            .AnyAsync(a =>
                a.AllocatrUserId == currentUserId
            );

        if (!allocatExists)
        {
            throw new UnauthorizedAccessException(
                "You do not have an Allocat profile."
            );
        }

        return await _db.ProjectAllocats
            .AsNoTracking()
            .Where(pa =>
                pa.AllocatProfileId == currentUserId &&
                pa.Project.UserId != currentUserId &&
                pa.RemovedAt == null &&
                (
                    pa.Status == ProjectAllocatStatus.Invited ||
                    pa.Status == ProjectAllocatStatus.Accepted
                )
            )
            .OrderByDescending(pa =>
                pa.InvitedAt
            )
            .Select(pa => new AllocatWorkProjectDto(
                pa.Project.Id,
                pa.Project.ProjectCode,
                pa.Project.Title,
                pa.Project.Description,
                pa.Project.Category,
                pa.Project.Status,
                pa.Project.Progress,
                pa.Project.Priority,
                pa.Project.StartDate,
                pa.Project.DueDate,
                pa.Project.Budget,
                pa.Project.Currency,
                pa.Project.AllocatAssignments.Any(a =>
                    a.Status == ProjectAllocatStatus.Accepted &&
                    a.RemovedAt == null
                ),
                pa.Project.CreatedAt,
                pa.Status,
                pa.InvitedAt,
                pa.RespondedAt
            ))
            .ToListAsync();
    }

    private static ProjectAllocatDto ToDto(
        ProjectAllocat projectAllocat)
    {
        return new ProjectAllocatDto(
            projectAllocat.ProjectId,
            projectAllocat.AllocatProfileId,
            projectAllocat.Status,
            projectAllocat.InvitedAt,
            projectAllocat.RespondedAt,
            projectAllocat.RemovedAt
        );
    }
}