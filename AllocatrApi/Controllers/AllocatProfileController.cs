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

    [HttpPost("create")]
    public async Task<IActionResult> CreateAllocatProfile([FromForm] CreateAllocatProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        if (!user.IsAllocat)
            return Forbid("User is not allocat");

        var existingProfile = await _allocatProfileService.GetAllocatProfileByUserIdAsync(user.Id);

        if (existingProfile != null)
            return BadRequest("Profile already exists");

        var allocatProfile = new AllocatProfile
        {
            AllocatrUserId = user.Id,
            IdNumber = dto.IdNumber,
            HourlyRate = dto.HourlyRate,
            YearsExperience = dto.YearsExperience,
            Bio = dto.Bio,
            Availability = "available",
            IsVisible = true,
            Skills = dto.Skills
        };

        var createdAllocatProfile = await _allocatProfileService.CreateAllocatProfileAsync(allocatProfile);

        var result = new AllocatProfileDto(
            createdAllocatProfile.AllocatrUserId,
            createdAllocatProfile.AllocatrUser.FullName,
            createdAllocatProfile.AllocatrUser.AvatarUrl,
            createdAllocatProfile.IdNumber,
            createdAllocatProfile.HourlyRate,
            createdAllocatProfile.Bio,
            createdAllocatProfile.Availability,
            createdAllocatProfile.YearsExperience,
            createdAllocatProfile.IsVisible,
            createdAllocatProfile.Skills
                .Select(ps => ps.Skill.Name)
                .ToList(),
            createdAllocatProfile.CreatedAt,
            createdAllocatProfile.UpdatedAt
        );

        return CreatedAtAction(
            nameof(GetAllocatProfileById),
            new { allocatProfileId = createdAllocatProfile.AllocatrUserId },
            result
        );
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyAllocatProfile()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
            return Unauthorized();

        var allocatProfile = await _allocatProfileService.GetAllocatProfileByUserIdAsync(user.Id);
        if (allocatProfile == null)
            return NotFound();

        return Ok(allocatProfile);
    }

    // GET api/allocats/profiles/{allocatProfileId}
    [AllowAnonymous]
    [HttpGet("{allocatProfileId:guid}")]
    public async Task<IActionResult> GetAllocatProfileById(Guid allocatProfileId)
    {
        var allocatProfile = await _allocatProfileService.GetAllocatProfileByUserIdAsync(allocatProfileId);
        if (allocatProfile == null)
            return NotFound();

        return Ok(allocatProfile);
    }

    // GET api/allocats/profiles
    [HttpGet("")]
    public async Task<IActionResult> GetAllAllocatProfiles()
    {
        var allocatProfiles = await _allocatProfileService.GetAllAllocatProfilesAsync();
        if (allocatProfiles == null)
            return NotFound();

        return Ok(allocatProfiles);
    }
}