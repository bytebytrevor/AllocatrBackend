namespace AllocatrApi.Dtos;

public record class UpdateProjectDto(
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
    List<Guid> Allocats,
    int? TasksCount,
    int? MessagesCount,
    DateTime? LastActivity,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency,
    string[] Attachments
);