using System.Text.Json.Serialization;
using AllocatrApi.Enums;
using AllocatrApi.Models;

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
    public DateTime? LeftAt { get; set; }

    [JsonIgnore]
    public string? LeaveReason { get; set; }
}
