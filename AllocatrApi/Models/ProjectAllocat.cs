using AllocatrApi.Models;

public class ProjectAllocat
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid AllocatId { get; set; }
    public AllocatrUser Allocat { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}