namespace AllocatrApi.Dtos;

public record class UpdateProjectDto(
    string Id,
    string ProjectCode,
    string Title,
    string Description,
    string Type,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateOnly? StartDate,
    DateOnly? DueDate,
    string Status,
    int Progress,
    string? Priority,
    string UserId,
    List<string>? Allocats,
    int? TasksCount,
    int? MessagesCount,
    DateTime? LastActivity,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency,
    string[] Attachments
);