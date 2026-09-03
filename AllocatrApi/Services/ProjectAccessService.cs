using AllocatrApi.Data;
using AllocatrApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class ProjectAccessService
{
    private readonly AllocatrDbContext _db;

    public ProjectAccessService(
        AllocatrDbContext db)
    {
        _db = db;
    }

    /* =====================================================
       CAN VIEW PROJECT

       A user may enter a project when:

       1. They own it

       OR

       2. They are an Allocat with an accepted,
          non-removed assignment.
    ===================================================== */

    public async Task<bool> CanViewProjectAsync(
        Guid projectId,
        Guid userId,
        bool isAllocat)
    {
        return await _db.Projects
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id == projectId &&
                (
                    p.UserId == userId ||
                    (
                        isAllocat &&
                        p.AllocatAssignments.Any(pa =>
                            pa.AllocatProfile.AllocatrUserId == userId &&
                            pa.Status == ProjectAllocatStatus.Accepted &&
                            pa.RemovedAt == null
                        )
                    )
                )
            );
    }

    /* =====================================================
       IS PROJECT OWNER

       Used for client/owner-only operations such as:

       - project settings
       - final approval
       - requesting changes
       - ownership-level management
    ===================================================== */

    public async Task<bool> IsProjectOwnerAsync(
        Guid projectId,
        Guid userId)
    {
        return await _db.Projects
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id == projectId &&
                p.UserId == userId
            );
    }

    /* =====================================================
       IS ACCEPTED ALLOCAT

       Used for Allocat execution permissions.

       The project must belong to somebody else. This keeps
       "project owner" and "Allocat working for a client"
       semantically separate.
    ===================================================== */

    public async Task<bool> IsAcceptedAllocatAsync(
        Guid projectId,
        Guid userId)
    {
        return await _db.Projects
            .AsNoTracking()
            .AnyAsync(p =>
                p.Id == projectId &&
                p.UserId != userId &&
                p.AllocatAssignments.Any(pa =>
                    pa.AllocatProfile.AllocatrUserId == userId &&
                    pa.Status == ProjectAllocatStatus.Accepted &&
                    pa.RemovedAt == null
                )
            );
    }
}