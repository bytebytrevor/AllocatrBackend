using System.ComponentModel.DataAnnotations;

namespace AllocatrApi.Dtos;

public sealed class UpdateTaskDueDateDto
{
    [Required]
    public DateTime? DueDate { get; init; }
}
