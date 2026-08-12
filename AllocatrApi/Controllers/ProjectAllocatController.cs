using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Authorize]
[Route("api/projects/{projectId:guid}/allocats")]
public class ProjectAllocatController : ControllerBase
{
    private readonly ProjectAllocatService _projectAllocatService;
    private readonly UserManager<AllocatrUser> _userManager;

    public ProjectAllocatController(
        ProjectAllocatService projectAllocatService,
        UserManager<AllocatrUser> userManager)
    {
        _projectAllocatService = projectAllocatService;
        _userManager = userManager;
    }

    // PUT:
    // api/projects/{projectId}/allocats/{allocatProfileId}/invite
    [HttpPut("{allocatProfileId:guid}/invite")]
    public async Task<IActionResult> InviteAllocat(
        Guid projectId,
        Guid allocatProfileId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        try
        {
            var result =
                await _projectAllocatService.InviteAllocatAsync(
                    projectId,
                    allocatProfileId,
                    user.Id
                );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PATCH:
    // api/projects/{projectId}/allocats/invite/accept
    [HttpPatch("invite/accept")]
    public async Task<IActionResult> AcceptInvite(
        Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        try
        {
            var result =
                await _projectAllocatService.AcceptInviteAsync(
                    projectId,
                    user.Id
                );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PATCH:
    // api/projects/{projectId}/allocats/invite/decline
    [HttpPatch("invite/decline")]
    public async Task<IActionResult> DeclineInvite(
        Guid projectId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        try
        {
            var result =
                await _projectAllocatService.DeclineInviteAsync(
                    projectId,
                    user.Id
                );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PATCH:
    // api/projects/{projectId}/allocats/{allocatProfileId}/remove
    [HttpPatch("{allocatProfileId:guid}/remove")]
    public async Task<IActionResult> RemoveAllocat(
        Guid projectId,
        Guid allocatProfileId)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        try
        {
            var result =
                await _projectAllocatService.RemoveAllocatAsync(
                    projectId,
                    allocatProfileId,
                    user.Id
                );

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}