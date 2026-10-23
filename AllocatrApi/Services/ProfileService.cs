using System.Text;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

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

        var countryName = await GetCountryNameAsync(user.CountryCode);
        return MapToDto(user, countryName);
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

        var fullName = request.FullName?.Trim() ?? string.Empty;

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

        var phoneNumber = CleanOptionalValue(request.PhoneNumber);
        var updateResidence = request.CountryCodeProvided || request.CityNameProvided;

        var countryCode = user.CountryCode;
        var cityName = user.CityName;
        var legacyLocation = user.Location;
        string? countryName;

        if (updateResidence)
        {
            countryCode = request.CountryCodeProvided
                ? CleanOptionalValue(request.CountryCode)?.ToUpperInvariant()
                : user.CountryCode;

            var countryChanged = !string.Equals(
                countryCode,
                user.CountryCode,
                StringComparison.Ordinal
            );

            // Changing country without sending a new city clears the old city.
            cityName = request.CityNameProvided
                ? CleanOptionalValue(request.CityName)
                : countryChanged ? null : user.CityName;

            if (countryCode != null &&
                (countryCode.Length != 2 ||
                 countryCode.Any(character => character < 'A' || character > 'Z')))
            {
                throw new ArgumentException(
                    "Country must use a two-letter code such as GB, ZA or ZW."
                );
            }

            if (cityName?.Length > 150)
            {
                throw new ArgumentException(
                    "City name cannot exceed 150 characters."
                );
            }

            if (countryCode == null && cityName != null)
            {
                throw new ArgumentException(
                    "Select a country before adding a city."
                );
            }

            // Residence accepts every seeded country, not just operating countries.
            countryName = await GetCountryNameAsync(countryCode);

            if (countryCode != null && countryName == null)
            {
                throw new ArgumentException(
                    "Select a country from the country catalogue."
                );
            }

            // The user has now explicitly supplied structured residence data.
            // Remove the old free-text source so clearing residence cannot revive it.
            // The response still generates Location for older readers (see MapToDto).
            legacyLocation = null;
        }
        else
        {
            countryName = await GetCountryNameAsync(countryCode);

            // Old forms can still update a legacy-only account. Once a residence
            // country exists, a free-text Location must not overwrite it.
            if (countryCode == null)
            {
                legacyLocation = CleanOptionalValue(request.Location);

                if (legacyLocation?.Length > 150)
                {
                    throw new ArgumentException(
                        "Location cannot exceed 150 characters."
                    );
                }
            }
        }

        // Assign only after all validation has passed.
        user.FullName = fullName;
        user.PhoneNumber = phoneNumber;
        user.CountryCode = countryCode;
        user.CityName = cityName;
        user.Location = legacyLocation;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                FormatIdentityErrors(result)
            );
        }

        return MapToDto(user, countryName);
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

    private async Task<string?> GetCountryNameAsync(string? countryCode)
    {
        if (string.IsNullOrWhiteSpace(countryCode))
        {
            return null;
        }

        return await _dbContext.Countries
            .AsNoTracking()
            .Where(country => country.Code == countryCode)
            .Select(country => country.Name)
            .SingleOrDefaultAsync();
    }

    private static ProfileDto MapToDto(
        AllocatrUser user,
        string? countryName
    )
    {
        // Location remains a display-only compatibility field for existing readers.
        // Generate it rather than squeezing the full label into its old DB length.
        var location = countryName == null
            ? user.Location
            : string.IsNullOrWhiteSpace(user.CityName)
                ? countryName
                : $"{user.CityName}, {countryName}";

        return new ProfileDto(
            user.Id,
            user.FullName,
            user.Email,
            user.PhoneNumber,
            location,
            user.AvatarUrl,
            user.CreatedAt,
            user.EmailConfirmed,
            user.IsAllocat,
            user.CountryCode,
            countryName,
            user.CityName
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
