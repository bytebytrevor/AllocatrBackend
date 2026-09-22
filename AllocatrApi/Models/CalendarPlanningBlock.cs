namespace AllocatrApi.Models;

public class CalendarPlanningBlock
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AllocatrUser User { get; set; } = null!;

    public Guid? ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
