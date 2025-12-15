namespace AllocatrApi.Dtos;

public record class TaskCommentDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Comment,
    Guid CreatedBy,
    Guid TaskId
);
