using System.Text;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Services;

public class ProfileService
{
    private const long MaxProfilePictureSize = 5 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/webp",
        };

    private readonly UserManager<AllocatrUser> _userManager;
    private readonly UserFileService _userFiles;
    private readonly IFileStorageService _storage;
    private readonly AllocatrDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public ProfileService(
        UserManager<AllocatrUser> userManager,
        UserFileService userFiles,
        IFileStorageService storage,
        AllocatrDbContext dbContext,
        IEmailService emailService,
        IConfiguration configuration
    )
    {
        _userManager = userManager;
        _userFiles = userFiles;
        _storage = storage;
        _dbContext = dbContext;
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

        StoredFile? oldAvatar = null;

        if (user.AvatarFileId.HasValue)
        {
            oldAvatar = await _dbContext.StoredFiles.FindAsync(
                user.AvatarFileId.Value
            );
        }

        var newAvatar =
            await _userFiles.UploadAvatarAsync(
                userId,
                file
            );

        var publicUrl =
            _storage.GetPublicUrl(
                newAvatar.Bucket,
                newAvatar.StoragePath
            );

        var cacheVersion =
            DateTimeOffset.UtcNow
                .ToUnixTimeMilliseconds();

        var avatarUrl =
            $"{publicUrl}?v={cacheVersion}";

        user.AvatarFileId = newAvatar.Id;
        user.AvatarUrl = avatarUrl;

        var updateResult =
            await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await _userFiles.DeleteStoredFileAsync(
                newAvatar
            );

            throw new InvalidOperationException(
                FormatIdentityErrors(updateResult)
            );
        }

        if (oldAvatar != null)
        {
            try
            {
                await _userFiles.DeleteStoredFileAsync(
                    oldAvatar
                );
            }
            catch (Exception error)
            {
                Console.Error.WriteLine(
                    $"Could not delete old avatar {oldAvatar.Id}: {error}"
                );
            }
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