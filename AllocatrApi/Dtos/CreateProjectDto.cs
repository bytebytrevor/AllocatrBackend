namespace AllocatrApi.Dtos;

public record class CreateProjectDto(
    string Title,
    string Description,
    string Type,
    string Category,
    DateTime? UpdatedAt,
    DateOnly? StartDate,
    DateOnly? DueDate,
    string? Priority,
    string UserId,
    List<ProjectMemberDto>? Allocats,
    DateTime? LastActivity,
    bool IsPublic,
    bool AllowBids,
    decimal? Budget,
    string Currency,
    string[] Attachments
);
