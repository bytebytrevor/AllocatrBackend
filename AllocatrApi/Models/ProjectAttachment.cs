public class ProjectAttachment
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string FileName { get; set; } = null!;
    public string FileUrl { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
