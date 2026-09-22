namespace AllocatrApi.Models;

public class CalendarFocusTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public AllocatrUser User { get; set; } = null!;

    public Guid TaskId { get; set; }
    public TaskItem Task { get; set; } = null!;

    public DateOnly WeekStart { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
