namespace AllocatrApi.Models;

public sealed class UserNotification
{
    public long Id { get; set; }
    public Guid RecipientId { get; set; }
    public Guid EventId { get; set; }
    public string Kind { get; set; } = "";
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = "";
    public string Text { get; set; } = "";
    public string Href { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public Guid? ActorId { get; set; }
    public Guid? TaskId { get; set; }
    public int ItemCount { get; set; } = 1;
    public DateTime? LastActivityAt { get; set; }
}
