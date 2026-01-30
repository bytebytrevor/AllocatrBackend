using AllocatrApi.Models;

namespace AllocatrApi.Dtos;


public record class TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    int Order,
    Guid? AssignedToId,
    DateTime? UpdatedAt,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt,
    ICollection<TaskComment>? Comments,
    Guid? CreatedByUserId,
    Guid ProjectId
);