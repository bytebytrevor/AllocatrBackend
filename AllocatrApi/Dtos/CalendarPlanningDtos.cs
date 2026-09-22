namespace AllocatrApi.Dtos;

public record CalendarPlanningBlockDto(
    Guid Id,
    Guid? ProjectId,
    Guid? TaskId,
    string? ProjectTitle,
    string Title,
    string? Notes,
    DateTime StartAt,
    DateTime EndAt
);

public class CreateCalendarPlanningBlockDto
{
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}

public class UpdateCalendarPlanningBlockDto
{
    public Guid? ProjectId { get; set; }
    public Guid? TaskId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
}

public record CalendarFocusTaskDto(
    Guid TaskId,
    Guid ProjectId,
    string ProjectTitle,
    string Title,
    string Status,
    DateTime? DueDate
);
