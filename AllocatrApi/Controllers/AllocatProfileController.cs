using System;
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

    // GET api/allocats/profiles
    [HttpGet("")]
    public async Task<IActionResult> GetAllAllocats()
    {
        return Ok();
    }

    // GET api/allocats/profiles/{allocatProfileId}
    [HttpGet("{allocatProfileId:guid}")]
    public async Task<IActionResult> GetAllocatProfileByUserId(string allocatProfileId)
    {
        var allocatProfile = await _allocatProfileService.GetAllocatProfileByUserIdAsync(allocatProfileId);
        if (allocatProfile == null)
            return NotFound();

        return Ok(allocatProfile);
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAllocatProfile(CreateAllocatProfileDto dto)
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
            Bio = dto.Bio,
            Availability = "available",
            YearsExperience = dto.YearsExperience,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdAllocatProfile = await _allocatProfileService.CreateAllocatProfileAsync(allocatProfile);

        var result = new AllocatProfileDto(
            createdAllocatProfile.AllocatrUserId,
            createdAllocatProfile.IdNumber,
            createdAllocatProfile.HourlyRate,
            createdAllocatProfile.Bio,
            createdAllocatProfile.Availability,
            createdAllocatProfile.YearsExperience,
            createdAllocatProfile.IsVisible,
            createdAllocatProfile.CreatedAt,
            createdAllocatProfile.UpdatedAt
        );

        return CreatedAtAction(
            nameof(GetAllocatProfileByUserId),
            new { allocatProfileId = createdAllocatProfile.AllocatrUserId },
            result
        );
    }
}
