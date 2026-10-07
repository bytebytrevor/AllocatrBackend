namespace AllocatrApi.Models;

public class UserDocument
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid StoredFileId { get; set; }

    public UserDocumentType DocumentType { get; set; }

    public UserDocumentReviewStatus ReviewStatus { get; set; } =
        UserDocumentReviewStatus.Pending;

    public string? ReviewNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAt { get; set; }

    public Guid? ReviewedByUserId { get; set; }

    public AllocatrUser User { get; set; } = null!;

    public StoredFile StoredFile { get; set; } = null!;
}

public enum UserDocumentType
{
    Identity,
    Qualification,
    Certification,
    ProfessionalLicense,
    Training,
    Other,
}

public enum UserDocumentReviewStatus
{
    Pending,
    Approved,
    Rejected,
}