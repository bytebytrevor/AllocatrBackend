namespace AllocatrApi.Models;

public class TaskComment
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string Comment { get; set; } = null!;
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public AllocatrUser CreatedBy { get; set; } = null!;
    // Optional for older clients; newer clients reuse this ID when retrying a post.
    public Guid? ClientRequestId { get; set; }
}
