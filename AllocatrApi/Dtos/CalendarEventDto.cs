namespace AllocatrApi.Dtos;

public record CalendarEventDto(
    string Id,
    string Type,
    Guid ProjectId,
    Guid? TaskId,
    string ProjectTitle,
    string? ProjectCode,
    string Title,
    string? Description,
    DateTime Start,
    DateTime? End,
    bool AllDay,
    string Status,
    string Relationship
);