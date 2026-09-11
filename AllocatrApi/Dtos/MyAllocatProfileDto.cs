namespace AllocatrApi.Dtos;

public record MyAllocatProfileDto(
    Guid AllocatrUserId,
    string FullName,
    string? AvatarUrl,
    string Email,
    string? IdNumber,
    string? Bio,
    string? Headline,
    string? Title,
    IReadOnlyList<SkillOptionDto> Skills,
    decimal Rating,
    int RatingCount,
    int CompletedProjects,
    bool Availability,
    string AvailabilityStatus,
    bool Verified,
    string? Location,
    decimal? HourlyRate,
    string Currency,
    int? ResponseTime,
    int Level,
    int ProfessionalScore,
    bool IsVisible,
    DateTime JoinedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<AllocatProjectSummaryDto> Projects

);