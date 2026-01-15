using AllocatrApi.Models;

public class ProjectMessage
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string SenderId { get; set; } = null!;
    public AllocatrUser Sender { get; set; } = null!;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsEdited { get; set; }
}
