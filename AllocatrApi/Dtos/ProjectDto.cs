using System.ComponentModel.DataAnnotations.Schema;

namespace AllocatrApi.Dtos;

public record class ProjectDto(
    Guid Id,
    string ProjectCode,
    string Title,
    string Description,
    string Category,
    string Status,
    int Progress,
    string? Priority,
    decimal? Budget,
    string Currency,
    DateOnly? StartDate,
    DateOnly? DueDate,
    ICollection<ProjectAllocat> AllocatAssignments
);
