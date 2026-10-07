using AllocatrApi.Models;

namespace AllocatrApi.Dtos;

public record UserDocumentDto(
    Guid Id,
    UserDocumentType DocumentType,
    UserDocumentReviewStatus ReviewStatus,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime? ReviewedAt
);