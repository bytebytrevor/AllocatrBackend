namespace AllocatrApi.Dtos;

public record AllocatProfileListItemDto(
    Guid AllocatrUserId,
    string FullName,
    string? AvatarUrl,
    string? Headline,
    string? Title,
    IReadOnlyList<SkillOptionDto> Skills,
    decimal Rating,
    int RatingCount,
    int CompletedProjects,
    bool Availability,
    bool Verified,
    string? Location,
    decimal? HourlyRate,
    string Currency,
    int? YearsExperience,
    int Level,
    int ProfessionalScore,
    DateTime JoinedAt
);