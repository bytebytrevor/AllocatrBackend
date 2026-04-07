namespace AllocatrApi.Dtos;

public record class AllocatProfileDto (
    string AllocatrUserId,
    string? IdNumber,
    decimal? HourlyRate,
    string? Bio,
    string?Availability,
    int? YearsExperience,
    bool IsVisible,
    DateTime CreatedAt,
    DateTime UpdatedAt    
);