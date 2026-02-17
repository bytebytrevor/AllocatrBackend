namespace AllocatrApi.Dtos;

public record class TaskCommentDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Comment,
    string CreatedById,
    Guid TaskItemId
);
