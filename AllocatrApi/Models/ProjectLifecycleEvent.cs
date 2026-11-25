namespace AllocatrApi.Models;

// Immutable business history survives a later re-invitation of the same membership row.
public sealed class ProjectLifecycleEvent
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid ActorId { get; set; }
    public string Kind { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string ProjectTitle { get; set; } = null!;
    public string ProjectCode { get; set; } = null!;
    public string ActorName { get; set; } = null!;
    public Guid[] RecipientIds { get; set; } = [];
}
