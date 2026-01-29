namespace AllocatrApi.Dtos;


public record class TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int Order,
    string AssignedTo,
    DateTime? UpdatedAt,
    DateTime CreatedAt,
    DateOnly? DueDate,
    DateTime? CompletedAt,
    List<TaskCommentDto>? Comments,
    Guid? CreatedByUserId,
    Guid ProjectId
);