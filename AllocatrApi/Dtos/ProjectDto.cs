using AllocatrApi.Models;

namespace AllocatrApi.Dtos;

public record ProjectDto(
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
    bool HasAllocat,
    DateTime CreatedAt,
    DateTime? CompletionRequestedAt,
    Guid? CompletionRequestedByAllocatId,
    DateTime? CompletedAt,
    DateOnly? StartDate,
    DateOnly? DueDate,
    ICollection<ProjectAllocat> AllocatAssignments,
    List<ProjectSkillDto> Skills
);