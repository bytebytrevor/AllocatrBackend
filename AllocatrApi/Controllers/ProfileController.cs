using System.Security.Claims;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly SupabaseService _supabase;
    private readonly UserManager<AllocatrUser> _userManager;

    public ProfileController(
        SupabaseService supabase,
        UserManager<AllocatrUser> userManager)
    {
        _supabase = supabase;
        _userManager = userManager;
    }

    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized();

        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        var path = $"{user.Id}/profile.png";

        await _supabase.Client
            .Storage
            .From("avatars")
            .Upload(bytes, path, new Supabase.Storage.FileOptions
            {
                Upsert = true
            });

        var publicUrl = _supabase.Client
            .Storage
            .From("avatars")
            .GetPublicUrl(path);

        // CACHE BUST
        var cacheBustedUrl = $"{publicUrl}?v={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        user.AvatarUrl = cacheBustedUrl;
        await _userManager.UpdateAsync(user);

        return Ok(new { avatarUrl = cacheBustedUrl });
    }
}


