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
[Route("api/projects")]
public sealed class ProjectLifecycleController(
    UserManager<AllocatrUser> users,
    ProjectLifecycleService lifecycle,
    ProjectService projects,
    ProjectInvitationEmailSender email,
    AllocatrDbContext db,
    ILogger<ProjectLifecycleController> logger
) : ControllerBase
{
    [HttpPatch("{projectId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid projectId, ProjectLifecycleRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        ProjectLifecycleResult result;
        try
        {
            result = await lifecycle.CancelAsync(
                projectId,
                user.Id,
                request.Reason,
                HttpContext.RequestAborted
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        // Database state and in-app notifications have already committed.
        if (result.Changed)
            await SendEmailsAsync(result.EventId);
        return Ok(await projects.GetAccessibleProjectByIdAsync(projectId, user.Id, user.IsAllocat));
    }

    [HttpPatch("{projectId:guid}/allocats/leave")]
    public async Task<IActionResult> Leave(Guid projectId, ProjectLifecycleRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user == null)
            return Unauthorized();
        if (!user.IsAllocat)
            return Forbid();
        ProjectLifecycleResult result;
        try
        {
            result = await lifecycle.LeaveAsync(
                projectId,
                user.Id,
                request.Reason,
                HttpContext.RequestAborted
            );
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        if (result.Changed)
            await SendEmailsAsync(result.EventId);
        return Ok(result);
    }

    private async Task SendEmailsAsync(Guid eventId)
    {
        // Do not turn a committed cancellation/departure into an API failure on SMTP errors.
        try
        {
            var recipients = await db.Set<ProjectLifecycleEvent>()
                .AsNoTracking()
                .Where(e => e.Id == eventId)
                .Select(e => e.RecipientIds)
                .SingleAsync();
            foreach (var recipientId in recipients)
            {
                try
                {
                    await email.SendLifecycleAsync(eventId, recipientId);
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "Lifecycle event {EventId} saved; email to {RecipientId} failed",
                        eventId,
                        recipientId
                    );
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Lifecycle event {EventId} saved; notification emails could not be prepared",
                eventId
            );
        }
    }
}
