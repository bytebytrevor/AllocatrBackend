namespace AllocatrApi.Dtos;

public sealed record ProjectAllocatRatingDto(
    Guid Id,
    Guid ProjectId,
    Guid AllocatId,
    int Rating,
    string? Comment,
    decimal AverageRating,
    int RatingCount,
    DateTime CreatedAt,
    DateTime UpdatedAt
);