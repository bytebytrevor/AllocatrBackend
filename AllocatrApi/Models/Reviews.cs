namespace AllocatrApi.Models;

public class Review
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid ReviewerId { get; set; }
    public AllocatrUser Reviewer { get; set; } = null!;

    public Guid AllocatProfileId { get; set; }
    public AllocatProfile AllocatProfile { get; set; } = null!;

    public decimal Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}