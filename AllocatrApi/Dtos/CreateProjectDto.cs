namespace AllocatrApi.Dtos;

public record class CreateProjectDto(
    string Title,
    string Description,
    string Category,
    List<string> Tags,
    DateOnly? StartDate,
    DateOnly? DueDate,
    string? Priority,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency
    // string[] Attachments
);
