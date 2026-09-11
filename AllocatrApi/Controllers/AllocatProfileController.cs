using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/allocats/profiles")]
[Authorize]
public class AllocatProfileController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly AllocatProfileService _allocatProfileService;

    public AllocatProfileController(
        UserManager<AllocatrUser> userManager,
        AllocatProfileService allocatProfileService)
    {
        _userManager = userManager;
        _allocatProfileService = allocatProfileService;
    }

    [HttpPost]
    public async Task<ActionResult<MyAllocatProfileDto>>
        CreateAllocatProfile(
            [FromBody] CreateAllocatProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Allocat access required",
                detail:
                    "This account is not registered as an Allocat."
            );
        }

        var profile =
            await _allocatProfileService
                .CreateAllocatProfileAsync(
                    user.Id,
                    dto
                );

        return CreatedAtAction(
            nameof(GetAllocatProfileById),
            new
            {
                allocatUserId =
                    profile.AllocatrUserId
            },
            profile
        );
    }

    [HttpGet("me")]
    public async Task<ActionResult<MyAllocatProfileDto>>
        GetMyAllocatProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var profile =
            await _allocatProfileService
                .GetMyAllocatProfileAsync(user.Id);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPut("me")]
    public async Task<ActionResult<MyAllocatProfileDto>>
        UpdateMyAllocatProfile(
            [FromBody] UpdateAllocatProfileDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        if (!user.IsAllocat)
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Allocat access required",
                detail:
                    "This account is not registered as an Allocat."
            );
        }

        var profile =
            await _allocatProfileService
                .UpdateAllocatProfileAsync(
                    user.Id,
                    dto
                );

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPatch("me/visibility")]
    public async Task<IActionResult> SetVisibility(
        [FromBody] SetAllocatVisibilityDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var updated =
            await _allocatProfileService
                .SetVisibilityAsync(
                    user.Id,
                    dto.IsVisible
                );

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPatch("me/availability")]
    public async Task<IActionResult> SetAvailability(
        [FromBody] SetAllocatAvailabilityDto dto)
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var updated =
            await _allocatProfileService
                .SetAvailabilityAsync(
                    user.Id,
                    dto.Availability
                );

        if (!updated)
        {
            return NotFound();
        }

        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("{allocatUserId:guid}")]
    public async Task<ActionResult<AllocatProfileDto>>
        GetAllocatProfileById(
            Guid allocatUserId)
    {
        var profile =
            await _allocatProfileService
                .GetPublicAllocatProfileAsync(
                    allocatUserId
                );

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<
        ActionResult<
            PagedResultDto<AllocatProfileListItemDto>
        >
    > GetAllAllocatProfiles(
        [FromQuery] AllocatProfileSearchDto query)
    {
        var profiles =
            await _allocatProfileService
                .GetAllAllocatProfilesAsync(query);

        return Ok(profiles);
    }
}