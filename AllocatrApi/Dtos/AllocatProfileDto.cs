using System.Collections;
using AllocatrApi.Models;

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
    List<String> Skills,
    DateTime CreatedAt,
    DateTime UpdatedAt
);