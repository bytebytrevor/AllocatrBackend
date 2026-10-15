namespace AllocatrApi.Dtos;

public record class TaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Status,
    string Priority,
    DateTime? DueDate,
    Guid? AssignedToId,
    string? AssignedToName,
    Guid CreatedByUserId,
    string? CreatedByName,
    string? AssignedToAvatarUrl = null
);