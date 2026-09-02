// using System.Security.Claims;
// using AllocatrApi.Models;
// using AllocatrApi.Services;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.AspNetCore.Mvc;

// namespace AllocatrApi.Controllers;

// [ApiController]
// [Route("api/profiles")]
// [Authorize]
// public class ProfileController : ControllerBase
// {
//     private readonly SupabaseService _supabase;
//     private readonly UserManager<AllocatrUser> _userManager;

//     public ProfileController(
//         SupabaseService supabase,
//         UserManager<AllocatrUser> userManager)
//     {
//         _supabase = supabase;
//         _userManager = userManager;
//     }

//     [HttpPost("profile-picture")]
//     public async Task<IActionResult> UploadProfilePicture(IFormFile file)
//     {
//         if (file == null || file.Length == 0)
//             return BadRequest("No file uploaded");

//         var user = await _userManager.GetUserAsync(User);
//         if (user == null)
//             return Unauthorized();

//         await using var ms = new MemoryStream();
//         await file.CopyToAsync(ms);
//         var bytes = ms.ToArray();

//         var path = $"{user.Id}/profile.png";

//         await _supabase.Client
//             .Storage
//             .From("avatars")
//             .Upload(bytes, path, new Supabase.Storage.FileOptions
//             {
//                 Upsert = true
//             });

//         var publicUrl = _supabase.Client
//             .Storage
//             .From("avatars")
//             .GetPublicUrl(path);

//         // CACHE BUST
//         var cacheBustedUrl = $"{publicUrl}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

//         user.AvatarUrl = cacheBustedUrl;
//         await _userManager.UpdateAsync(user);

//         return Ok(new { avatarUrl = cacheBustedUrl });
//     }
// }


using System.Security.Claims;
using AllocatrApi.Dtos;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    /* --------------------------------------------------------
     * GET CURRENT PROFILE
     * -------------------------------------------------------- */

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetProfile()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        var profile = await _profileService.GetProfileAsync(
            userId.Value
        );

        if (profile == null)
        {
            return NotFound(
                new { message = "Profile not found." }
            );
        }

        return Ok(profile);
    }

    /* --------------------------------------------------------
     * UPDATE CURRENT PROFILE
     * -------------------------------------------------------- */

    [HttpPatch("me")]
    public async Task<ActionResult<ProfileDto>> UpdateProfile(
        [FromBody] UpdateProfileDto request
    )
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var profile =  await _profileService.UpdateProfileAsync(
                userId.Value,
                request
            );

            if (profile == null)
            {
                return NotFound(new { message = "Profile not found."});
            }

            return Ok(profile);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(
                new { message = ex.Message }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new { message = ex.Message }
            );
        }
    }

    /* --------------------------------------------------------
    * EMAIL VERIFICATION
    * -------------------------------------------------------- */

    [HttpPost("me/email-verification")]
    public async Task<IActionResult> SendEmailVerification()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var sent =
                await _profileService.SendEmailVerificationAsync(
                    userId.Value
                );

            if (!sent)
            {
                return NotFound(
                    new { message = "Profile not found." }
                );
            }

            return Ok(
                new
                {
                    message = "Verification email sent."
                }
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(
                new { message = ex.Message }
            );
        }
    }

    /* --------------------------------------------------------
     * PROFILE PICTURE
     * -------------------------------------------------------- */

    [HttpPost("profile-picture")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> UploadProfilePicture(
        [FromForm] IFormFile file
    )
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            var avatarUrl = await _profileService.UploadProfilePictureAsync(
                userId.Value,
                file
            );

            if (avatarUrl == null)
            {
                return NotFound(
                    new { message = "Profile not found." }
                );
            }

            return Ok( new { avatarUrl });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new {message = ex.Message});
        }
    }

    /* --------------------------------------------------------
     * CURRENT USER
     * -------------------------------------------------------- */

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
            return null;

        return userId;
    }
}