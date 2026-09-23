namespace AllocatrApi.Dtos;

public sealed record SubmitProjectRatingsDto(
    List<SubmitAllocatRatingDto> Ratings
);

public sealed record SubmitAllocatRatingDto(
    Guid AllocatId,
    int Rating,
    string? Comment
);