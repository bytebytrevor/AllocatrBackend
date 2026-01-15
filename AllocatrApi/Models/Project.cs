using AllocatrApi.Models;

public class Project
{
    public Guid Id { get; set; }

    // Business identity
    public string ProjectCode { get; set; } = null!;

    // Basic info
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Category { get; set; } = null!;

    // Status & progress
    public string Status { get; set; } = "Draft";
    public int Progress { get; set; } = 0;
    public string? Priority { get; set; }

    // Ownership
    public string UserId { get; set; } = null!;
    public AllocatrUser User { get; set; } = null!;

    // Dates
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? DueDate { get; set; }

    // Visibility & rules
    public bool IsPublic { get; set; }
    public bool AllowBids { get; set; }

    // Budget
    public decimal? Budget { get; set; }
    public string Currency { get; set; } = "USD";

    // Navigation collections
    public ICollection<ProjectTag> Tags { get; set; } = new List<ProjectTag>();
    public ICollection<ProjectAllocat> AllocatAssignments { get; set; } = new List<ProjectAllocat>();
    public ICollection<ProjectAttachment> Attachments { get; set; } = new List<ProjectAttachment>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<ProjectMessage> Messages { get; set; } = new List<ProjectMessage>();
}

