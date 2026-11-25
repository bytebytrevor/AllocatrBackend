using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/allocats")]
public class ProjectAllocatController : ControllerBase
{
    private readonly ProjectAllocatService _projectAllocatService;
    private readonly AllocatrDbContext _db;
    private readonly NotificationService _notifications;
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly ProjectInvitationEmailSender _invitationEmail;
    private readonly ILogger<ProjectAllocatController> _logger;

    public ProjectAllocatController(
        ProjectAllocatService projectAllocatService,
        UserManager<AllocatrUser> userManager,
        ProjectInvitationEmailSender invitationEmail,
        ILogger<ProjectAllocatController> logger,
        AllocatrDbContext db,
        NotificationService notifications
    )
    {
        _projectAllocatService = projectAllocatService;
        _userManager = userManager;
        _invitationEmail = invitationEmail;
        _logger = logger;
        _db = db;
        _notifications = notifications;
    }

    [HttpPut("{allocatProfileId:guid}/invite")]
    public async Task<IActionResult> InviteAllocat(Guid projectId, Guid allocatProfileId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await WithNotificationAsync(
                () =>
                    _projectAllocatService.InviteAllocatAsync(projectId, allocatProfileId, user.Id),
                NotificationEvent.InvitationReceived,
                projectId,
                user,
                allocatProfileId
            );
            // Invitation persistence has already succeeded. SMTP failures must
            // not report a failed invite or cause the client to invite again.
            try
            {
                await _invitationEmail.SendAsync(projectId, allocatProfileId, user.Id);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Invitation was saved but its email could not be sent for project {ProjectId}, Allocat {AllocatId}",
                    projectId,
                    allocatProfileId
                );
            }
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("invite/accept")]
    public async Task<IActionResult> AcceptInvite(Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await WithNotificationAsync(
                () => _projectAllocatService.AcceptInviteAsync(projectId, user.Id),
                NotificationEvent.InvitationAccepted,
                projectId,
                user
            );
            await SendResponseEmailAsync(projectId, user.Id, accepted: true);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("invite/decline")]
    public async Task<IActionResult> DeclineInvite(Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await WithNotificationAsync(
                () => _projectAllocatService.DeclineInviteAsync(projectId, user.Id),
                NotificationEvent.InvitationDeclined,
                projectId,
                user
            );
            await SendResponseEmailAsync(projectId, user.Id, accepted: false);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPatch("{allocatProfileId:guid}/remove")]
    public async Task<IActionResult> RemoveAllocat(Guid projectId, Guid allocatProfileId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await _projectAllocatService.RemoveAllocatAsync(
                projectId,
                allocatProfileId,
                user.Id
            );
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpGet("{allocatProfileId:guid}")]
    public async Task<ActionResult<ProjectAllocatDto>> GetProjectAllocat(
        Guid projectId,
        Guid allocatProfileId
    )
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await _projectAllocatService.GetProjectAllocatAsync(
                projectId,
                allocatProfileId,
                user.Id
            );
            if (result == null)
                return NotFound();
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProjectAllocats(Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await _projectAllocatService.GetProjectAllocatsAsync(projectId, user.Id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("members")]
    public async Task<ActionResult<List<ProjectAllocatMemberDto>>> GetProjectMembers(Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await _projectAllocatService.GetProjectMembersAsync(projectId, user.Id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("/api/allocats/me/projects")]
    public async Task<ActionResult<List<AllocatWorkProjectDto>>> GetMyWorkProjects()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        try
        {
            var result = await _projectAllocatService.GetMyWorkProjectsAsync(user.Id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private async Task SendResponseEmailAsync(Guid projectId, Guid allocatUserId, bool accepted)
    {
        // The response and its in-app notification have already committed.
        // SMTP failure must not turn a saved response into an API failure.
        try
        {
            await _invitationEmail.SendResponseAsync(projectId, allocatUserId, accepted);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Project invitation response was saved but its email could not be sent for project {ProjectId}, Allocat {AllocatId}, response {Response}",
                projectId,
                allocatUserId,
                accepted ? "accepted" : "declined"
            );
        }
    }

    private async Task<T> WithNotificationAsync<T>(
        Func<Task<T>> mutation,
        NotificationEvent kind,
        Guid projectId,
        AllocatrUser actor,
        Guid? recipientId = null
    )
    {
        // Existing service authorization and status checks remain authoritative.
        // The shared scoped DbContext keeps its SaveChanges inside this transaction.
        await using var transaction = await _db.Database.BeginTransactionAsync(
            HttpContext.RequestAborted
        );
        var result = await mutation();
        var project = await _db
            .Projects.AsNoTracking()
            .Where(p => p.Id == projectId)
            .Select(p => new { p.UserId, p.Title })
            .SingleAsync(HttpContext.RequestAborted);
        await _notifications.QueueAsync(
            kind,
            Guid.NewGuid(),
            actor.Id,
            new[] { recipientId ?? project.UserId },
            projectId,
            project.Title,
            actor.FullName ?? "A project member",
            HttpContext.RequestAborted
        );
        await _db.SaveChangesAsync(HttpContext.RequestAborted);
        await transaction.CommitAsync(HttpContext.RequestAborted);
        return result;
    }
}
