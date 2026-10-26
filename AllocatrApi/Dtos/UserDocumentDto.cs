using AllocatrApi.Models;

namespace AllocatrApi.Dtos;

// Owner-only document response. Do not use this DTO for public profiles.
public record UserDocumentDto(
    Guid Id,
    UserDocumentType DocumentType,
    UserDocumentReviewStatus ReviewStatus,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    DateTime CreatedAt,
    DateTime? ReviewedAt,
    string? Title = null,
    string? IssuingInstitution = null
);
