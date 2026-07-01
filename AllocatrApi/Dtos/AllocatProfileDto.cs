namespace AllocatrApi.Dtos;

public record class AllocatProfileDto(
    Guid AllocatrUserId,
    string? FullName,
    string? IdNumber,
    decimal? HourlyRate,
    string? Bio,
    string? Availability,
    int? YearsExperience,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime UpdatedAt
);