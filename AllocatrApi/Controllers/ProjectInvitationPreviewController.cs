using System.Security.Claims;
using AllocatrApi.Data;
using AllocatrApi.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectInvitationPreviewController(AllocatrDbContext db) : ControllerBase
{
    // A pending invitation permits this brief only, not the project workspace.
    [HttpGet("{projectId:guid}/invitation-preview")]
    public async Task<IActionResult> Get(Guid projectId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var summary = await (
            from assignment in db.ProjectAllocats.AsNoTracking()
            join client in db.Users.AsNoTracking() on assignment.Project.UserId equals client.Id
            join country in db.Countries.AsNoTracking() on client.CountryCode equals country.Code into clientCountries
            from country in clientCountries.DefaultIfEmpty()
            where assignment.ProjectId == projectId
                && assignment.AllocatProfile.AllocatrUserId == userId
                && assignment.Status == ProjectAllocatStatus.Invited
                && assignment.RemovedAt == null
            select new
            {
                assignment.Project.Id,
                assignment.Project.ProjectCode,
                assignment.Project.Title,
                assignment.Project.Description,
                assignment.Project.Category,
                assignment.Project.Priority,
                assignment.Project.Budget,
                assignment.Project.Currency,
                assignment.Project.StartDate,
                assignment.Project.DueDate,
                assignment.InvitedAt,
                Client = new
                {
                    client.FullName,
                    City = client.CityName,
                    Country = country != null ? country.Name : null,
                    client.CountryCode
                }
            }).FirstOrDefaultAsync(cancellationToken);

        // The same result for another user's invitation and an absent project.
        if (summary is null) return NotFound();

        var skills = await (
            from projectSkill in db.ProjectSkills.AsNoTracking()
            join skill in db.Skills.AsNoTracking() on projectSkill.SkillId equals skill.Id
            where projectSkill.ProjectId == projectId
            orderby skill.Name
            select new { skill.Id, skill.Name }
        ).Distinct().ToListAsync(cancellationToken);

        return Ok(new
        {
            summary.Id,
            summary.ProjectCode,
            summary.Title,
            summary.Description,
            summary.Category,
            summary.Priority,
            summary.Budget,
            summary.Currency,
            summary.StartDate,
            summary.DueDate,
            summary.InvitedAt,
            summary.Client,
            Skills = skills
        });
    }
}
