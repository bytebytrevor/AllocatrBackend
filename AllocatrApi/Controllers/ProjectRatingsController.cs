using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/ratings")]
[Authorize]
public class ProjectRatingsController : ControllerBase
{
    private readonly UserManager<AllocatrUser>
        _userManager;

    private readonly ReviewService
        _reviewService;

    public ProjectRatingsController(
        UserManager<AllocatrUser> userManager,
        ReviewService reviewService)
    {
        _userManager = userManager;
        _reviewService = reviewService;
    }

    [HttpPut]
    public async Task<
        ActionResult<
            IReadOnlyList<ProjectAllocatRatingDto>
        >
    > SubmitProjectRatings(
        Guid projectId,
        [FromBody] SubmitProjectRatingsDto dto)
    {
        var user =
            await _userManager
                .GetUserAsync(User);

        if (user == null)
        {
            return Unauthorized();
        }

        var ratings =
            await _reviewService
                .SubmitProjectRatingsAsync(
                    projectId,
                    user.Id,
                    dto
                );

        return Ok(ratings);
    }
}