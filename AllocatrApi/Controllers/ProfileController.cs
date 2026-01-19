using System.Security.Claims;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly SupabaseService _supabase;

    public ProfileController(SupabaseService supabase)
    {
        _supabase = supabase;
    }

    [HttpPost("profile-picture")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        // Get logged-in user's ID
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Read file into memory
        await using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();

        // Use a fixed path per user to overwrite
        var path = $"/{userId}/profile.png";

        // Upload to Supabase (with overwrite)
        await _supabase.Client
            .Storage
            .From("avatars")
            .Upload(bytes, path, new Supabase.Storage.FileOptions
            {
                Upsert = true // overwrite if exists
            });

        // Get public URL
        var publicUrl = _supabase.Client
            .Storage
            .From("avatars")
            .GetPublicUrl(path);

        return Ok(new { path, publicUrl });
    }
}
