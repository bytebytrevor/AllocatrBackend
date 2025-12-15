namespace AllocatrApi.Dtos;


public record class TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int Order,
    DateTime CreatedAt,
    DateOnly? DueDate,
    DateTime? UpdatedAt,
    DateTime? CompletedAt,
    List<TaskCommentDto>? Comments,
    Guid? CreatedByUserId,
    Guid ProjectId
);