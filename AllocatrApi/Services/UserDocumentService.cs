using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class UserDocumentService
{
    private const long MaxDocumentSize = 10 * 1024 * 1024;
    private const int MaxProfessionalDocuments = 8;
    private const int DownloadUrlLifetimeSeconds = 300;
    private const int MaxMetadataLength = 200;

    private static readonly HashSet<string> AllowedDocumentTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "image/webp",
        };

    private readonly AllocatrDbContext _dbContext;
    private readonly UserFileService _userFiles;
    private readonly IFileStorageService _storage;

    public UserDocumentService(
        AllocatrDbContext dbContext,
        UserFileService userFiles,
        IFileStorageService storage)
    {
        _dbContext = dbContext;
        _userFiles = userFiles;
        _storage = storage;
    }

    public async Task<IReadOnlyList<UserDocumentDto>> GetMyDocumentsAsync(Guid userId)
    {
        var documents = await _dbContext.UserDocuments
            .AsNoTracking()
            .Include(document => document.StoredFile)
            .Where(document =>
                document.UserId == userId &&
                document.StoredFile.Status == StoredFileStatus.Ready)
            .OrderByDescending(document => document.CreatedAt)
            .ToListAsync();

        return documents.Select(MapToDto).ToList();
    }

    public async Task<UserDocumentDto> UploadDocumentAsync(
        Guid userId,
        UserDocumentType documentType,
        IFormFile file,
        string? title = null,
        string? issuingInstitution = null)
    {
        ValidateDocumentType(documentType);
        ValidateDocument(file);

        // Validate before uploading bytes or creating a stored-file record.
        var metadata = ValidateMetadata(documentType, title, issuingInstitution);
        await ValidateUploadRulesAsync(userId, documentType);

        StoredFile? storedFile = null;
        try
        {
            storedFile = await _userFiles.UploadUserDocumentAsync(
                userId, documentType, file);

            var document = new UserDocument
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoredFileId = storedFile.Id,
                DocumentType = documentType,
                Title = metadata.Title,
                IssuingInstitution = metadata.IssuingInstitution,
                ReviewStatus = UserDocumentReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.UserDocuments.Add(document);
            await _dbContext.SaveChangesAsync();
            document.StoredFile = storedFile;
            return MapToDto(document);
        }
        catch
        {
            if (storedFile?.Status == StoredFileStatus.Ready)
            {
                try
                {
                    await _userFiles.DeleteStoredFileAsync(storedFile);
                }
                catch
                {
                    // Preserve the original error; orphan cleanup can retry.
                }
            }
            throw;
        }
    }

    public async Task<UserDocumentDto?> UpdateProfessionalDocumentMetadataAsync(
        Guid userId, Guid documentId, UpdateUserDocumentMetadataDto dto)
    {
        var document = await _dbContext.UserDocuments
            .AsNoTracking()
            .Include(item => item.StoredFile)
            .FirstOrDefaultAsync(item =>
                item.Id == documentId && item.UserId == userId &&
                item.StoredFile.Status == StoredFileStatus.Ready);

        if (document == null) return null;
        if (document.DocumentType == UserDocumentType.Identity)
            throw new ArgumentException("Identity document details cannot be edited here.");

        var metadata = ValidateMetadata(document.DocumentType, dto.Title, dto.IssuingInstitution);
        if (document.Title == metadata.Title &&
            document.IssuingInstitution == metadata.IssuingInstitution)
            return MapToDto(document);

        // Compare the snapshot in the UPDATE so a concurrent review or edit
        // cannot silently be overwritten. File evidence remains unchanged.
        var updated = await _dbContext.UserDocuments
            .Where(item => item.Id == documentId && item.UserId == userId &&
                item.DocumentType == document.DocumentType &&
                item.StoredFile.Status == StoredFileStatus.Ready &&
                item.Title == document.Title &&
                item.IssuingInstitution == document.IssuingInstitution &&
                item.ReviewStatus == document.ReviewStatus &&
                item.ReviewedAt == document.ReviewedAt &&
                item.ReviewedByUserId == document.ReviewedByUserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(item => item.Title, metadata.Title)
                .SetProperty(item => item.IssuingInstitution, metadata.IssuingInstitution)
                .SetProperty(item => item.ReviewStatus, UserDocumentReviewStatus.Pending)
                .SetProperty(item => item.ReviewNotes, (string?)null)
                .SetProperty(item => item.ReviewedAt, (DateTime?)null)
                .SetProperty(item => item.ReviewedByUserId, (Guid?)null));

        if (updated == 0)
            throw new InvalidOperationException(
                "This credential changed while you were editing. Refresh your documents and try again.");

        document.Title = metadata.Title;
        document.IssuingInstitution = metadata.IssuingInstitution;
        document.ReviewStatus = UserDocumentReviewStatus.Pending;
        document.ReviewNotes = null;
        document.ReviewedAt = null;
        document.ReviewedByUserId = null;
        return MapToDto(document);
    }

    public async Task<string?> GetDownloadUrlAsync(Guid userId, Guid documentId)
    {
        var document = await _dbContext.UserDocuments
            .AsNoTracking()
            .Include(document => document.StoredFile)
            .FirstOrDefaultAsync(document =>
                document.Id == documentId && document.UserId == userId);

        if (document == null || document.StoredFile.Status != StoredFileStatus.Ready)
            return null;

        return await _storage.CreateSignedUrlAsync(
            document.StoredFile.Bucket,
            document.StoredFile.StoragePath,
            DownloadUrlLifetimeSeconds);
    }

    public async Task<bool> DeleteDocumentAsync(Guid userId, Guid documentId)
    {
        var document = await _dbContext.UserDocuments
            .Include(document => document.StoredFile)
            .FirstOrDefaultAsync(document =>
                document.Id == documentId && document.UserId == userId);

        if (document == null) return false;

        if (document.ReviewStatus == UserDocumentReviewStatus.Approved)
            throw new InvalidOperationException("Approved documents cannot be deleted.");

        await _userFiles.DeleteStoredFileAsync(document.StoredFile);
        _dbContext.UserDocuments.Remove(document);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private async Task ValidateUploadRulesAsync(Guid userId, UserDocumentType documentType)
    {
        if (documentType == UserDocumentType.Identity)
        {
            var alreadyHasIdentity = await _dbContext.UserDocuments
                .AnyAsync(document =>
                    document.UserId == userId &&
                    document.DocumentType == UserDocumentType.Identity &&
                    document.StoredFile.Status != StoredFileStatus.Deleted &&
                    document.StoredFile.Status != StoredFileStatus.Failed);

            if (alreadyHasIdentity)
                throw new InvalidOperationException("An identity document has already been uploaded.");
            return;
        }

        var professionalDocumentCount = await _dbContext.UserDocuments
            .CountAsync(document =>
                document.UserId == userId &&
                document.DocumentType != UserDocumentType.Identity &&
                document.StoredFile.Status != StoredFileStatus.Deleted &&
                document.StoredFile.Status != StoredFileStatus.Failed);

        if (professionalDocumentCount >= MaxProfessionalDocuments)
            throw new InvalidOperationException(
                $"You can upload a maximum of {MaxProfessionalDocuments} professional documents.");
    }

    private static (string? Title, string? IssuingInstitution) ValidateMetadata(
        UserDocumentType documentType, string? title, string? issuingInstitution)
    {
        // Identity evidence never contributes public qualification metadata.
        if (documentType == UserDocumentType.Identity) return (null, null);

        title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        issuingInstitution = string.IsNullOrWhiteSpace(issuingInstitution)
            ? null : issuingInstitution.Trim();

        if (documentType == UserDocumentType.Qualification)
        {
            if (title == null)
                throw new ArgumentException("Enter the qualification title.", nameof(title));
            if (issuingInstitution == null)
                throw new ArgumentException(
                    "Enter the institution that awarded the qualification.", nameof(issuingInstitution));
        }

        if (title is { Length: > MaxMetadataLength })
            throw new ArgumentException(
                $"The document title cannot exceed {MaxMetadataLength} characters.", nameof(title));
        if (issuingInstitution is { Length: > MaxMetadataLength })
            throw new ArgumentException(
                $"The issuing institution cannot exceed {MaxMetadataLength} characters.", nameof(issuingInstitution));

        return (title, issuingInstitution);
    }

    private static void ValidateDocumentType(UserDocumentType documentType)
    {
        if (!Enum.IsDefined(documentType))
            throw new ArgumentException("The selected document type is invalid.", nameof(documentType));
    }

    private static void ValidateDocument(IFormFile file)
    {
        if (file == null || file.Length <= 0)
            throw new ArgumentException("No document was uploaded.", nameof(file));
        if (file.Length > MaxDocumentSize)
            throw new ArgumentException("Documents cannot exceed 10 MB.", nameof(file));
        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !AllowedDocumentTypes.Contains(file.ContentType))
            throw new ArgumentException("Documents must be PDF, JPEG, PNG or WebP.", nameof(file));
    }

    private static UserDocumentDto MapToDto(UserDocument document)
    {
        return new UserDocumentDto(
            document.Id,
            document.DocumentType,
            document.ReviewStatus,
            document.StoredFile.OriginalFileName,
            document.StoredFile.ContentType,
            document.StoredFile.SizeBytes,
            document.CreatedAt,
            document.ReviewedAt,
            document.Title,
            document.IssuingInstitution);
    }
}
