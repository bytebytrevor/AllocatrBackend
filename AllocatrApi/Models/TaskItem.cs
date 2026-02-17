using AllocatrApi.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "todo";
    public string Priority { get; set; } = null!;
    public int Order { get; set; }
    public string? AssignedToId { get; set; }
    public AllocatrUser? AssignedTo { get; set; }
    public string CreatedByUserId { get; set; } = null!;
    public AllocatrUser CreatedByUser { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
}
