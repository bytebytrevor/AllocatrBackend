namespace AllocatrApi.Models;

public enum ProjectFileContext
{
    Project,
    Task,
    Comment,
}

public class ProjectFile
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Guid StoredFileId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public Guid? CommentId { get; set; }
    public TaskComment? Comment { get; set; }
    public Guid? TaskId { get; set; }
    // Preserve the source label when a task is later deleted.
    public string? TaskTitle { get; set; }
    public ProjectFileContext Context { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // StoredFile.DeletedAt records logical deletion. This records byte removal.
    public DateTime? StorageDeletedAt { get; set; }
    public Project Project { get; set; } = null!;
    public StoredFile StoredFile { get; set; } = null!;
    public TaskItem? Task { get; set; }
}
