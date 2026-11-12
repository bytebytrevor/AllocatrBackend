using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Claims;
using AllocatrApi.Data;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects")]
public sealed class ProjectInvitationPreviewController(AllocatrDbContext db, IFileStorageService storage, ILogger<ProjectInvitationPreviewController> logger) : ControllerBase
{
    // A pending invitation permits this brief only, not the project workspace.
    [HttpGet("{projectId:guid}/invitation-preview")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
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

        var files = await InvitationFiles(projectId, userId)
            .OrderBy(file => file.CreatedAt).ThenBy(file => file.Id)
            .Select(file => new
            {
                file.Id,
                file.StoredFile.OriginalFileName,
                file.StoredFile.ContentType,
                file.StoredFile.SizeBytes
            }).ToListAsync(cancellationToken);

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
            Skills = skills,
            Files = files
        });
    }

    [HttpGet("{projectId:guid}/invitation-preview/files/{fileId:guid}/download")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Download(Guid projectId, Guid fileId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return Unauthorized();

        var file = await InvitationFiles(projectId, userId)
            .Where(file => file.Id == fileId)
            .Select(file => new { file.StoredFile.Bucket, file.StoredFile.StoragePath })
            .FirstOrDefaultAsync(cancellationToken);
        if (file is null) return NotFound();

        const int lifetimeSeconds = 300;
        var expiresAt = DateTime.UtcNow.AddSeconds(lifetimeSeconds);
        try
        {
            var url = await storage.CreateSignedUrlAsync(file.Bucket, file.StoragePath, lifetimeSeconds);
            if (string.IsNullOrWhiteSpace(url)) throw new IOException("Empty storage URL.");
            // Recheck after storage signing: a revoked invitation or deleted
            // file must not publish a download link from an earlier read.
            if (!await InvitationFiles(projectId, userId).AnyAsync(item => item.Id == fileId, cancellationToken))
                return NotFound();
            return Ok(new { Url = url, ExpiresAt = expiresAt });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not sign invitation file {FileId} for project {ProjectId}", fileId, projectId);
            return Problem(statusCode: 503, title: "File temporarily unavailable",
                detail: "This file could not be downloaded. Please try again.");
        }
    }

    // Read-only access to the client's project-level supporting files. Pending
    // invitations never grant task/comment access, upload or delete permissions.
    private IQueryable<ProjectFile> InvitationFiles(Guid projectId, Guid userId) =>
        db.ProjectFiles.AsNoTracking().Where(file =>
            file.ProjectId == projectId
            && file.Context == ProjectFileContext.Project
            && file.TaskId == null && file.CommentId == null
            && file.UploadedByUserId == file.Project.UserId
            && file.StoredFile.Status == StoredFileStatus.Ready
            && db.ProjectAllocats.Any(assignment =>
                assignment.ProjectId == projectId
                && assignment.AllocatProfile.AllocatrUserId == userId
                && assignment.Status == ProjectAllocatStatus.Invited
                && assignment.RemovedAt == null));

}
