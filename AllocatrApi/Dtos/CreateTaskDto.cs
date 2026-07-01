namespace AllocatrApi.Dtos;

public record class CreateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    Guid? AssignedToId,
    string Priority = "standard"
);
