namespace AllocatrApi.Dtos;

public record class CreateAllocatProfileDto (
    string? IdNumber,
    decimal? HourlyRate,
    string? Bio,
    int? YearsExperience
);