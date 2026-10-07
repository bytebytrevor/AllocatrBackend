namespace AllocatrApi.Models;

public class StoredFile
{
    public Guid Id { get; set; }

    public Guid UploadedByUserId { get; set; }

    public string Bucket { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string OriginalFileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long SizeBytes { get; set; }

    public StoredFileStatus Status { get; set; } =
        StoredFileStatus.Uploading;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadyAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public AllocatrUser UploadedByUser { get; set; } = null!;
}

public enum StoredFileStatus
{
    Uploading,
    Ready,
    Failed,
    Quarantined,
    Deleted,
}