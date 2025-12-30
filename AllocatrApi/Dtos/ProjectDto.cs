namespace AllocatrApi.Dtos;

public record class ProjectDto(
    Guid Id,
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
    Guid UserId,
    List<Guid> AllocatIds,
    int? TasksCount,
    int? MessagesCount,
    DateTime? LastActivity,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency,
    string[] Attachments
);
