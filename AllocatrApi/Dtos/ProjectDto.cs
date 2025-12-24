namespace AllocatrApi.Dtos;

public record class ProjectDto(
    string Id,
    string ProjectCode,
    string Title,
    string Description,
    string Category,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateOnly? StartDate,
    DateOnly? DueDate,
    string Status,
    int Progress,
    string? Priority,
    string UserId,
    List<string>? AllocatIds,
    int? TasksCount,
    int? MessagesCount,
    DateTime? LastActivity,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency,
    string[] Attachments
);
