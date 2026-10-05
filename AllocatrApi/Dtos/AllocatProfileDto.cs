namespace AllocatrApi.Dtos;

public record AllocatProfileDto(
    Guid AllocatrUserId,
    string FullName,
    string? AvatarUrl,
    string? Bio,
    string? Headline,
    string? Title,
    IReadOnlyList<string> Skills,
    decimal Rating,
    int RatingCount,
    int CompletedProjects,
    bool Availability,
    string AvailabilityStatus,
    bool Verified,
    string? Location,
    decimal? HourlyRate,
    string Currency,
    int? YearsExperience,
    int? ResponseTime,
    int Level,
    int ProfessionalScore,
    DateTime JoinedAt,
    IReadOnlyList<AllocatProjectSummaryDto> Projects
);