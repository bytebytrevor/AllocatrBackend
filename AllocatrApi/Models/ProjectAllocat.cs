using AllocatrApi.Models;

public class ProjectAllocat
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string AllocatId { get; set; } = null!;
    public AllocatrUser Allocat { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}