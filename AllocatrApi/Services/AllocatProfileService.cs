using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Constants;

namespace AllocatrApi.Services;

public class AllocatProfileService
{
    private static readonly HashSet<string> AllowedAvailability =
    [
        "available",
        "busy",
        "unavailable"
    ];

    private readonly AllocatrDbContext _db;

    public AllocatProfileService(AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<MyAllocatProfileDto> CreateAllocatProfileAsync(
        Guid userId,
        CreateAllocatProfileDto dto)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new KeyNotFoundException("User not found.");

        if (!user.IsAllocat)
        {
            throw new InvalidOperationException(
                "The user is not registered as an Allocat."
            );
        }

        var profileExists = await _db.AllocatProfiles
            .AnyAsync(a => a.AllocatrUserId == userId);

        if (profileExists)
        {
            throw new InvalidOperationException(
                "An Allocat profile already exists for this user."
            );
        }

        var input = await NormalizeAndValidateAsync(
            userId,
            dto.IdNumber,
            dto.Title,
            dto.Headline,
            dto.Bio,
            dto.HourlyRate,
            dto.Currency,
            dto.Availability,
            dto.YearsExperience,
            dto.SkillIds
        );

        var now = DateTime.UtcNow;

        var profile = new AllocatProfile
        {
            AllocatrUserId = userId,
            IdNumber = input.IdNumber,
            Title = input.Title,
            Headline = input.Headline,
            Bio = input.Bio,
            HourlyRate = input.HourlyRate,
            Currency = input.Currency,
            Availability = input.Availability,
            YearsExperience = input.YearsExperience,

            AverageRating = 0m,
            RatingCount = 0,

            IsVerified = false,
            AverageResponseTimeMinutes = null,
            Level = 1,

            IsVisible = true,

            CreatedAt = now,
            UpdatedAt = now,

            Skills = input.SkillIds
                .Select(skillId => new AllocatProfileSkill
                {
                    AllocatProfileId = userId,
                    SkillId = skillId
                })
                .ToList()
        };

        _db.AllocatProfiles.Add(profile);

        await _db.SaveChangesAsync();

        return await GetMyAllocatProfileAsync(userId)
            ?? throw new InvalidOperationException(
                "The profile was created but could not be reloaded."
            );
    }

    public async Task<MyAllocatProfileDto?> GetMyAllocatProfileAsync(
        Guid userId)
    {
        var data = await GetProfileDataAsync(
            userId,
            publicView: false
        );

        return data == null
            ? null
            : ToMyProfileDto(data);
    }

    public async Task<AllocatProfileDto?> GetPublicAllocatProfileAsync(
        Guid allocatUserId)
    {
        var data = await GetProfileDataAsync(
            allocatUserId,
            publicView: true
        );

        return data == null
            ? null
            : ToPublicProfileDto(data);
    }

    public async Task<PagedResultDto<AllocatProfileListItemDto>>
        GetAllAllocatProfilesAsync(AllocatProfileSearchDto request)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, 50);

        if (
            request.MaxHourlyRate.HasValue &&
            request.MaxHourlyRate.Value < 0
        )
        {
            throw new ArgumentException(
                "Maximum hourly rate cannot be negative."
            );
        }

        if (
            request.MinYearsExperience.HasValue &&
            request.MinYearsExperience.Value < 0
        )
        {
            throw new ArgumentException(
                "Minimum years of experience cannot be negative."
            );
        }

        var query = _db.AllocatProfiles
            .AsNoTracking()
            .Where(a =>
                a.IsVisible &&
                a.AllocatrUser.IsAllocat
            );

        var search = CleanOptional(
            request.Search,
            150
        );

        if (search != null)
        {
            var pattern = $"%{search}%";

            query = query.Where(a =>
                EF.Functions.ILike(
                    a.AllocatrUser.FullName,
                    pattern
                ) ||
                (
                    a.Title != null &&
                    EF.Functions.ILike(
                        a.Title,
                        pattern
                    )
                ) ||
                (
                    a.Headline != null &&
                    EF.Functions.ILike(
                        a.Headline,
                        pattern
                    )
                ) ||
                a.Skills.Any(s =>
                    EF.Functions.ILike(
                        s.Skill.Name,
                        pattern
                    )
                )
            );
        }

        var location = CleanOptional(
            request.Location,
            150
        );

        if (location != null)
        {
            var locationPattern = $"%{location}%";

            query = query.Where(a =>
                a.AllocatrUser.Location != null &&
                EF.Functions.ILike(
                    a.AllocatrUser.Location,
                    locationPattern
                )
            );
        }

        if (request.SkillId.HasValue)
        {
            query = query.Where(a =>
                a.Skills.Any(s =>
                    s.SkillId == request.SkillId.Value
                )
            );
        }

        if (request.MaxHourlyRate.HasValue)
        {
            query = query.Where(a =>
                a.HourlyRate.HasValue &&
                a.HourlyRate.Value <=
                    request.MaxHourlyRate.Value
            );
        }

        if (request.MinYearsExperience.HasValue)
        {
            query = query.Where(a =>
                a.YearsExperience.HasValue &&
                a.YearsExperience.Value >=
                    request.MinYearsExperience.Value
            );
        }

        if (request.AvailableOnly == true)
        {
            query = query.Where(a =>
                a.Availability == "available"
            );
        }

        var totalCount = await query.CountAsync();

        var profiles = await query
            .OrderByDescending(a => a.IsVerified)
            .ThenByDescending(a => a.AverageRating)
            .ThenByDescending(a => a.RatingCount)
            .ThenByDescending(a => a.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new ProfileListData
            {
                AllocatrUserId = a.AllocatrUserId,
                FullName = a.AllocatrUser.FullName,
                AvatarUrl = a.AllocatrUser.AvatarUrl,
                Location = a.AllocatrUser.Location,
                JoinedAt = a.AllocatrUser.CreatedAt,

                HasIdNumber =
                    a.IdNumber != null &&
                    a.IdNumber != "",

                Title = a.Title,
                Headline = a.Headline,
                Bio = a.Bio,

                HourlyRate = a.HourlyRate,
                Currency = a.Currency,

                Availability = a.Availability,
                YearsExperience = a.YearsExperience,

                AverageRating = a.AverageRating,
                RatingCount = a.RatingCount,

                IsVerified = a.IsVerified,
                Level = a.Level,

                // Skills = a.Skills
                //     .OrderBy(s => s.Skill.Name)
                //     .Select(s => s.Skill.Name)
                //     .ToList(),

                Skills = a.Skills
                    .OrderBy(s => s.Skill.Name)
                    .Select(s => new SkillOptionDto(
                        s.SkillId,
                        s.Skill.Name,
                        s.Skill.SkillCategoryId,
                        s.Skill.SkillCategory.Name
                    ))
                    .ToList(),

                CompletedProjects =
                    a.ProjectAssignments.Count(pa =>
                        pa.Status ==
                            ProjectAllocatStatus.Accepted &&
                        pa.RemovedAt == null &&
                        pa.Project.Status ==
                            ProjectStatuses.Completed
                    )
            })
            .ToListAsync();

        var items = profiles
            .Select(ToListItemDto)
            .ToList();

        return new PagedResultDto<AllocatProfileListItemDto>(
            items,
            page,
            pageSize,
            totalCount
        );
    }

    public async Task<MyAllocatProfileDto?> UpdateAllocatProfileAsync(
        Guid userId,
        UpdateAllocatProfileDto dto)
    {
        var profile = await _db.AllocatProfiles
            .Include(a => a.Skills)
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == userId
            );

        if (profile == null)
            return null;

        var input = await NormalizeAndValidateAsync(
            userId,
            dto.IdNumber,
            dto.Title,
            dto.Headline,
            dto.Bio,
            dto.HourlyRate,
            dto.Currency,
            dto.Availability,
            dto.YearsExperience,
            dto.SkillIds
        );

        profile.IdNumber = input.IdNumber;
        profile.Title = input.Title;
        profile.Headline = input.Headline;
        profile.Bio = input.Bio;

        profile.HourlyRate = input.HourlyRate;
        profile.Currency = input.Currency;

        profile.Availability = input.Availability;
        profile.YearsExperience = input.YearsExperience;

        UpdateSkills(
            profile,
            input.SkillIds
        );

        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return await GetMyAllocatProfileAsync(userId);
    }

    public async Task<bool> SetVisibilityAsync(
        Guid userId,
        bool isVisible)
    {
        var profile = await _db.AllocatProfiles
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == userId
            );

        if (profile == null)
            return false;

        profile.IsVisible = isVisible;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SetAvailabilityAsync(
        Guid userId,
        string availability)
    {
        var normalizedAvailability =
            NormalizeAvailability(availability);

        var profile = await _db.AllocatProfiles
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == userId
            );

        if (profile == null)
            return false;

        profile.Availability = normalizedAvailability;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RefreshRatingAsync(
        Guid allocatUserId)
    {
        var profile = await _db.AllocatProfiles
            .FirstOrDefaultAsync(a =>
                a.AllocatrUserId == allocatUserId
            );

        if (profile == null)
            return false;

        var ratings = _db.Reviews
            .AsNoTracking()
            .Where(r =>
                r.AllocatProfileId == allocatUserId
            )
            .Select(r =>
                (decimal)r.Rating
            );

        var ratingCount =
            await ratings.CountAsync();

        var averageRating =
            ratingCount == 0
                ? 0m
                : await ratings.AverageAsync();

        profile.RatingCount =
            ratingCount;

        profile.AverageRating =
            Math.Round(
                averageRating,
                2
            );

        await _db.SaveChangesAsync();

        return true;
    }

    private async Task<ProfileData?> GetProfileDataAsync(
        Guid allocatUserId,
        bool publicView)
    {
        var query = _db.AllocatProfiles
            .AsNoTracking()
            .Where(a =>
                a.AllocatrUserId == allocatUserId &&
                a.AllocatrUser.IsAllocat
            );

        if (publicView)
            query = query.Where(a => a.IsVisible);

        return await query
            .Select(a => new ProfileData
            {
                AllocatrUserId = a.AllocatrUserId,

                FullName = a.AllocatrUser.FullName,
                AvatarUrl = a.AllocatrUser.AvatarUrl,
                Email = a.AllocatrUser.Email ?? string.Empty,
                Location = a.AllocatrUser.Location,
                JoinedAt = a.AllocatrUser.CreatedAt,

                IdNumber = a.IdNumber,
                HasIdNumber =
                    a.IdNumber != null &&
                    a.IdNumber != "",

                Title = a.Title,
                Headline = a.Headline,
                Bio = a.Bio,

                HourlyRate = a.HourlyRate,
                Currency = a.Currency,

                Availability = a.Availability,
                YearsExperience = a.YearsExperience,

                AverageRating = a.AverageRating,
                RatingCount = a.RatingCount,

                IsVerified = a.IsVerified,
                AverageResponseTimeMinutes =
                    a.AverageResponseTimeMinutes,
                Level = a.Level,

                IsVisible = a.IsVisible,

                CreatedAt = a.CreatedAt,
                UpdatedAt = a.UpdatedAt,

                SkillOptions = a.Skills
                    .OrderBy(s => s.Skill.Name)
                    .Select(s => new SkillOptionDto(
                        s.SkillId,
                        s.Skill.Name,
                        s.Skill.SkillCategoryId,
                        s.Skill.SkillCategory.Name
                    ))
                    .ToList(),

                CompletedProjects =
                    a.ProjectAssignments.Count(pa =>
                        pa.Status ==
                            ProjectAllocatStatus.Accepted &&
                        pa.RemovedAt == null &&
                        pa.Project.Status ==
                            ProjectStatuses.Completed
                    ),

                Projects = a.ProjectAssignments
                    .Where(pa =>
                        pa.Status ==
                            ProjectAllocatStatus.Accepted &&
                        pa.RemovedAt == null &&
                        pa.Project.Status ==
                            ProjectStatuses.Completed &&
                        (
                            !publicView ||
                            pa.Project.IsPublic
                        )
                    )
                    .OrderByDescending(pa =>
                        pa.Project.UpdatedAt ??
                        pa.Project.CreatedAt
                    )
                    .Take(6)
                    .Select(pa =>
                        new AllocatProjectSummaryDto(
                            pa.Project.Id,
                            pa.Project.ProjectCode,
                            pa.Project.Title,
                            pa.Project.Category,
                            pa.Project.Status
                        )
                    )
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    private async Task<NormalizedProfileInput>
        NormalizeAndValidateAsync(
            Guid userId,
            string? idNumber,
            string? title,
            string? headline,
            string? bio,
            decimal? hourlyRate,
            string? currency,
            string? availability,
            int? yearsExperience,
            IReadOnlyCollection<Guid>? skillIds)
    {
        var normalizedIdNumber = NormalizeRequired(
                idNumber,
                "ID number",
                50
            )
            .ToUpperInvariant();

        var normalizedTitle = CleanOptional(
            title,
            120
        );

        var normalizedHeadline = CleanOptional(
            headline,
            180
        );

        var normalizedBio = CleanOptional(
            bio,
            500
        );

        var normalizedCurrency =
            NormalizeCurrency(currency);

        var normalizedAvailability =
            NormalizeAvailability(availability);

        if (
            hourlyRate.HasValue &&
            hourlyRate.Value < 0
        )
        {
            throw new ArgumentException(
                "Hourly rate cannot be negative."
            );
        }

        if (
            hourlyRate.HasValue &&
            hourlyRate.Value > 1_000_000m
        )
        {
            throw new ArgumentException(
                "Hourly rate is outside the supported range."
            );
        }

        if (
            yearsExperience.HasValue &&
            (
                yearsExperience.Value < 0 ||
                yearsExperience.Value > 80
            )
        )
        {
            throw new ArgumentException(
                "Years of experience must be between 0 and 80."
            );
        }

        var normalizedSkillIds = (skillIds ?? [])
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (normalizedSkillIds.Count == 0)
        {
            throw new ArgumentException(
                "At least one skill is required."
            );
        }

        if (normalizedSkillIds.Count > 20)
        {
            throw new ArgumentException(
                "A profile cannot contain more than 20 skills."
            );
        }

        var existingSkillCount = await _db.Skills
            .CountAsync(s =>
                normalizedSkillIds.Contains(s.Id)
            );

        if (existingSkillCount != normalizedSkillIds.Count)
        {
            throw new ArgumentException(
                "One or more selected skills do not exist."
            );
        }

        var duplicateIdNumber =
            await _db.AllocatProfiles.AnyAsync(a =>
                a.IdNumber == normalizedIdNumber &&
                a.AllocatrUserId != userId
            );

        if (duplicateIdNumber)
        {
            throw new InvalidOperationException(
                "This ID number is already associated with another Allocat profile."
            );
        }

        return new NormalizedProfileInput(
            normalizedIdNumber,
            normalizedTitle,
            normalizedHeadline,
            normalizedBio,
            hourlyRate,
            normalizedCurrency,
            normalizedAvailability,
            yearsExperience,
            normalizedSkillIds
        );
    }

    private void UpdateSkills(
        AllocatProfile profile,
        IReadOnlyCollection<Guid> skillIds)
    {
        var requestedSkillIds = skillIds.ToHashSet();
        var existingLinks = profile.Skills.ToList();

        foreach (var existingLink in existingLinks)
        {
            if (requestedSkillIds.Contains(existingLink.SkillId))
                continue;

            profile.Skills.Remove(existingLink);
            _db.Remove(existingLink);
        }

        var existingSkillIds = profile.Skills
            .Select(s => s.SkillId)
            .ToHashSet();

        foreach (var skillId in requestedSkillIds)
        {
            if (existingSkillIds.Contains(skillId))
                continue;

            profile.Skills.Add(
                new AllocatProfileSkill
                {
                    AllocatProfileId =
                        profile.AllocatrUserId,
                    SkillId = skillId
                }
            );
        }
    }

    private static string NormalizeAvailability(
        string? availability)
    {
        var value = string.IsNullOrWhiteSpace(availability)
            ? "available"
            : availability
                .Trim()
                .ToLowerInvariant();

        if (!AllowedAvailability.Contains(value))
        {
            throw new ArgumentException(
                "Availability must be available, busy or unavailable."
            );
        }

        return value;
    }

    private static string NormalizeCurrency(
        string? currency)
    {
        var value = string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency
                .Trim()
                .ToUpperInvariant();

        if (
            value.Length != 3 ||
            !value.All(char.IsLetter)
        )
        {
            throw new ArgumentException(
                "Currency must be a valid three-letter currency code."
            );
        }

        return value;
    }

    private static string NormalizeRequired(
        string? value,
        string fieldName,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"{fieldName} is required."
            );
        }

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed {maxLength} characters."
            );
        }

        return normalized;
    }

    private static string? CleanOptional(
        string? value,
        int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim();

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters."
            );
        }

        return normalized;
    }

    private static int CalculateProfessionalScore(
        bool hasIdNumber,
        string? title,
        string? headline,
        string? bio,
        decimal? hourlyRate,
        int? yearsExperience,
        string? avatarUrl,
        string? location,
        int skillCount)
    {
        var score = 0;

        if (hasIdNumber)
            score += 10;

        if (!string.IsNullOrWhiteSpace(title))
            score += 10;

        if (!string.IsNullOrWhiteSpace(headline))
            score += 10;

        if (!string.IsNullOrWhiteSpace(bio))
            score += 20;

        score += skillCount switch
        {
            >= 3 => 20,
            2 => 14,
            1 => 7,
            _ => 0
        };

        if (
            hourlyRate.HasValue &&
            hourlyRate.Value > 0
        )
        {
            score += 10;
        }

        if (yearsExperience.HasValue)
            score += 10;

        if (!string.IsNullOrWhiteSpace(avatarUrl))
            score += 5;

        if (!string.IsNullOrWhiteSpace(location))
            score += 5;

        return Math.Min(score, 100);
    }

    private static AllocatProfileDto ToPublicProfileDto(
        ProfileData data)
    {
        var skillNames = data.SkillOptions
            .Select(skill => skill.Name)
            .ToList();

        var professionalScore =
            CalculateProfessionalScore(
                data.HasIdNumber,
                data.Title,
                data.Headline,
                data.Bio,
                data.HourlyRate,
                data.YearsExperience,
                data.AvatarUrl,
                data.Location,
                skillNames.Count
            );

        return new AllocatProfileDto(
            data.AllocatrUserId,
            data.FullName,
            data.AvatarUrl,
            data.Bio,
            data.Headline,
            data.Title,
            skillNames,
            data.AverageRating,
            data.RatingCount,
            data.CompletedProjects,
            data.Availability == "available",
            data.Availability,
            data.IsVerified,
            data.Location,
            data.HourlyRate,
            data.Currency,
            data.AverageResponseTimeMinutes,
            data.Level,
            professionalScore,
            data.JoinedAt,
            data.Projects
        );
    }

    private static MyAllocatProfileDto ToMyProfileDto(
        ProfileData data)
    {
        var professionalScore =
            CalculateProfessionalScore(
                data.HasIdNumber,
                data.Title,
                data.Headline,
                data.Bio,
                data.HourlyRate,
                data.YearsExperience,
                data.AvatarUrl,
                data.Location,
                data.SkillOptions.Count
            );

        return new MyAllocatProfileDto(
            data.AllocatrUserId,
            data.FullName,
            data.AvatarUrl,
            data.Email,
            data.IdNumber,
            data.Bio,
            data.Headline,
            data.Title,
            data.SkillOptions,
            data.AverageRating,
            data.RatingCount,
            data.CompletedProjects,
            data.Availability == "available",
            data.Availability,
            data.IsVerified,
            data.Location,
            data.HourlyRate,
            data.Currency,
            data.AverageResponseTimeMinutes,
            data.Level,
            professionalScore,
            data.IsVisible,
            data.JoinedAt,
            data.CreatedAt,
            data.UpdatedAt,
            data.Projects
        );
    }

    private static AllocatProfileListItemDto ToListItemDto(
        ProfileListData data)
    {
        var professionalScore =
            CalculateProfessionalScore(
                data.HasIdNumber,
                data.Title,
                data.Headline,
                data.Bio,
                data.HourlyRate,
                data.YearsExperience,
                data.AvatarUrl,
                data.Location,
                data.Skills.Count
            );

        return new AllocatProfileListItemDto(
            data.AllocatrUserId,
            data.FullName,
            data.AvatarUrl,
            data.Headline,
            data.Title,
            data.Skills,
            data.AverageRating,
            data.RatingCount,
            data.CompletedProjects,
            data.Availability == "available",
            data.IsVerified,
            data.Location,
            data.HourlyRate,
            data.Currency,
            data.YearsExperience,
            data.Level,
            professionalScore,
            data.JoinedAt
        );
    }

    private sealed class ProfileData
    {
        public Guid AllocatrUserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? Location { get; set; }

        public string? IdNumber { get; set; }
        public bool HasIdNumber { get; set; }

        public string? Title { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }

        public decimal? HourlyRate { get; set; }
        public string Currency { get; set; } = "USD";

        public string Availability { get; set; } = "available";
        public int? YearsExperience { get; set; }

        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }

        public bool IsVerified { get; set; }
        public int? AverageResponseTimeMinutes { get; set; }
        public int Level { get; set; }

        public bool IsVisible { get; set; }

        public DateTime JoinedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int CompletedProjects { get; set; }

        public List<SkillOptionDto> SkillOptions { get; set; } = [];

        public List<AllocatProjectSummaryDto> Projects { get; set; } = [];
    }

    private sealed class ProfileListData
    {
        public Guid AllocatrUserId { get; set; }

        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string? Location { get; set; }

        public bool HasIdNumber { get; set; }

        public string? Title { get; set; }
        public string? Headline { get; set; }
        public string? Bio { get; set; }

        public decimal? HourlyRate { get; set; }
        public string Currency { get; set; } = "USD";

        public string Availability { get; set; } = "available";
        public int? YearsExperience { get; set; }

        public decimal AverageRating { get; set; }
        public int RatingCount { get; set; }

        public bool IsVerified { get; set; }
        public int Level { get; set; }
        public int CompletedProjects { get; set; }

        public DateTime JoinedAt { get; set; }

        // public List<string> Skills { get; set; } = [];
        public List<SkillOptionDto> Skills { get; set; } = [];
    }

    private sealed record NormalizedProfileInput(
        string IdNumber,
        string? Title,
        string? Headline,
        string? Bio,
        decimal? HourlyRate,
        string Currency,
        string Availability,
        int? YearsExperience,
        IReadOnlyCollection<Guid> SkillIds
    );
}