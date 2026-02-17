using System;

namespace AllocatrApi.Models;

public class TaskComment
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string Comment { get; set; } = null!;

    // Task relationship
    public Guid TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    // User relationship (Created By)
    public string CreatedById { get; set; } = null!;
    public AllocatrUser CreatedBy { get; set; } = null!;
}
