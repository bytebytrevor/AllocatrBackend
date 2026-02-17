namespace AllocatrApi.Dtos;

public record class CreateTaskDto(
    string Title,
    string? Description,
    DateTime? DueDate,
    string? AssignedToId,
    string Priority = "standard"
);
