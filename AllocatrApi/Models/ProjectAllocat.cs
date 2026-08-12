using AllocatrApi.Models;
using AllocatrApi.Enums;

public class ProjectAllocat
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid AllocatProfileId { get; set; }
    public AllocatProfile AllocatProfile { get; set; } = null!;

    public ProjectAllocatStatus Status { get; set; }

    public DateTime InvitedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? RemovedAt { get; set; }    
}