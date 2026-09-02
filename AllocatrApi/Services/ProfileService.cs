using System.Text;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Services;

public class ProfileService
{
    private const long MaxProfilePictureSize =
        5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
        };

    private readonly UserManager<AllocatrUser> _userManager;
    private readonly SupabaseService _supabase;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ProfileService(
        UserManager<AllocatrUser> userManager,
        SupabaseService supabase,
        IEmailService emailService,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _supabase = supabase;
        _emailService = emailService;
        _configuration = configuration;
    }

    /* --------------------------------------------------------
     * READ
     * -------------------------------------------------------- */

    public async Task<ProfileDto?> GetProfileAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString()
        );

        if (user == null)
        {
            return null;
        }

        return MapToDto(user);
    }

    /* --------------------------------------------------------
     * UPDATE
     * -------------------------------------------------------- */

    public async Task<ProfileDto?> UpdateProfileAsync(
        Guid userId,
        UpdateProfileDto request
    )
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString()
        );

        if (user == null)
        {
            return null;
        }

        var fullName = request.FullName.Trim();

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ArgumentException(
                "Full name is required."
            );
        }

        if (fullName.Length > 150)
        {
            throw new ArgumentException(
                "Full name cannot exceed 150 characters."
            );
        }

        var location = CleanOptionalValue(
            request.Location
        );

        if (location?.Length > 150)
        {
            throw new ArgumentException(
                "Location cannot exceed 150 characters."
            );
        }

        var phoneNumber = CleanOptionalValue(
            request.PhoneNumber
        );

        user.FullName = fullName;
        user.Location = location;
        user.PhoneNumber = phoneNumber;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                FormatIdentityErrors(result)
            );
        }

        return MapToDto(user);
    }

     /* --------------------------------------------------------
     * EMAIL VERIFICATION
     * -------------------------------------------------------- */

    public async Task<bool> SendEmailVerificationAsync(
        Guid userId
    )
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString()
        );

        if (user == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(user.Email))
        {
            throw new InvalidOperationException(
                "This account does not have an email address."
            );
        }

        if (user.EmailConfirmed)
        {
            throw new InvalidOperationException(
                "Your email address is already verified."
            );
        }

        var token =
            await _userManager.GenerateEmailConfirmationTokenAsync(
                user
            );

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var frontendUrl =
            _configuration["Frontend:BaseUrl"];

        if (string.IsNullOrWhiteSpace(frontendUrl))
        {
            throw new InvalidOperationException(
                "Frontend URL has not been configured."
            );
        }

        var verificationUrl =
            $"{frontendUrl.TrimEnd('/')}/verify-email" +
            $"?userId={user.Id}" +
            $"&token={encodedToken}";

        await _emailService.SendEmailVerificationAsync(
            user.Email,
            user.FullName,
            verificationUrl
        );

        return true;
    }

    /* --------------------------------------------------------
     * PROFILE PICTURE
     * -------------------------------------------------------- */

    public async Task<string?> UploadProfilePictureAsync(
        Guid userId,
        IFormFile file
    )
    {
        var user = await _userManager.FindByIdAsync(
            userId.ToString()
        );

        if (user == null)
        {
            return null;
        }

        ValidateProfilePicture(file);

        await using var memoryStream =
            new MemoryStream();

        await file.CopyToAsync(memoryStream);

        var bytes = memoryStream.ToArray();

        // Actual uploaded bytes may be JPEG, PNG or WebP,
        var extension = GetImageExtension(
            file.ContentType
        );

        var path =
            $"{user.Id}/profile{extension}";

        await _supabase.Client
            .Storage
            .From("avatars")
            .Upload(
                bytes,
                path,
                new Supabase.Storage.FileOptions
                {
                    Upsert = true
                }
            );

        var publicUrl = _supabase.Client
            .Storage
            .From("avatars")
            .GetPublicUrl(path);

        // Cache busting to keep filename same whenever the avatar is replaced.
        var cacheVersion =
            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var avatarUrl =
            $"{publicUrl}?v={cacheVersion}";

        user.AvatarUrl = avatarUrl;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException(
                FormatIdentityErrors(updateResult)
            );
        }

        return avatarUrl;
    }

    /* --------------------------------------------------------
     * VALIDATION
     * -------------------------------------------------------- */

    private static void ValidateProfilePicture(
        IFormFile file
    )
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException(
                "No image was uploaded."
            );
        }

        if (file.Length > MaxProfilePictureSize)
        {
            throw new ArgumentException(
                "The profile picture cannot exceed 5 MB."
            );
        }

        if (!AllowedImageTypes.Contains(file.ContentType))
        {
            throw new ArgumentException(
                "Profile pictures must be JPEG, PNG or WebP."
            );
        }
    }

    private static string GetImageExtension(
        string contentType
    )
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new ArgumentException(
                "Unsupported image type."
            ),
        };
    }

    /* --------------------------------------------------------
     * HELPERS
     * -------------------------------------------------------- */

    private static ProfileDto MapToDto(
        AllocatrUser user
    )
    {
        return new ProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            user.Location,
            user.AvatarUrl,
            user.CreatedAt,
            user.EmailConfirmed,
            user.IsAllocat
        );
    }

    private static string? CleanOptionalValue(
        string? value
    )
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    private static string FormatIdentityErrors(
        IdentityResult result
    )
    {
        var errors = result.Errors
            .Select(error => error.Description);

        return string.Join(" ", errors);
    }
}