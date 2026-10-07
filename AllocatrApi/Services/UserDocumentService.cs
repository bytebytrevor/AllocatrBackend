using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class UserDocumentService
{
    private const long MaxDocumentSize =
        10 * 1024 * 1024;

    private const int MaxProfessionalDocuments = 8;

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
        IFileStorageService storage
    )
    {
        _dbContext = dbContext;
        _userFiles = userFiles;
        _storage = storage;
    }

    /* --------------------------------------------------------
     * READ
     * -------------------------------------------------------- */

    public async Task<IReadOnlyList<UserDocumentDto>>
        GetMyDocumentsAsync(
            Guid userId
        )
    {
        var documents =
            await _dbContext.UserDocuments
                .AsNoTracking()
                .Include(document => document.StoredFile)
                .Where(
                    document =>
                        document.UserId == userId &&
                        document.StoredFile.Status ==
                            StoredFileStatus.Ready
                )
                .OrderByDescending(
                    document => document.CreatedAt
                )
                .ToListAsync();

        return documents
            .Select(MapToDto)
            .ToList();
    }

    /* --------------------------------------------------------
     * UPLOAD
     * -------------------------------------------------------- */

    public async Task<UserDocumentDto>
        UploadDocumentAsync(
            Guid userId,
            UserDocumentType documentType,
            IFormFile file
        )
    {
        ValidateDocument(file);

        await ValidateUploadRulesAsync(
            userId,
            documentType
        );

        StoredFile? storedFile = null;

        try
        {
            storedFile =
                await _userFiles.UploadUserDocumentAsync(
                    userId,
                    documentType,
                    file
                );

            var document = new UserDocument
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                StoredFileId = storedFile.Id,
                DocumentType = documentType,
                ReviewStatus =
                    UserDocumentReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow,
            };

            _dbContext.UserDocuments.Add(
                document
            );

            await _dbContext.SaveChangesAsync();

            document.StoredFile = storedFile;

            return MapToDto(document);
        }
        catch
        {
            if (
                storedFile != null &&
                storedFile.Status ==
                    StoredFileStatus.Ready
            )
            {
                try
                {
                    await _userFiles
                        .DeleteStoredFileAsync(
                            storedFile
                        );
                }
                catch
                {
                    // Preserve the original upload/domain error.
                    // Failed cleanup can be handled by orphan cleanup later.
                }
            }

            throw;
        }
    }

    /* --------------------------------------------------------
     * DOWNLOAD
     * -------------------------------------------------------- */

    public async Task<string?>
        GetDownloadUrlAsync(
            Guid userId,
            Guid documentId
        )
    {
        var document =
            await _dbContext.UserDocuments
                .AsNoTracking()
                .Include(document => document.StoredFile)
                .FirstOrDefaultAsync(
                    document =>
                        document.Id == documentId &&
                        document.UserId == userId
                );

        if (
            document == null ||
            document.StoredFile.Status !=
                StoredFileStatus.Ready
        )
        {
            return null;
        }

        return await _storage.CreateSignedUrlAsync(
            document.StoredFile.Bucket,
            document.StoredFile.StoragePath,
            300
        );
    }

    /* --------------------------------------------------------
     * DELETE
     * -------------------------------------------------------- */

    public async Task<bool> DeleteDocumentAsync(
        Guid userId,
        Guid documentId
    )
    {
        var document =
            await _dbContext.UserDocuments
                .Include(document => document.StoredFile)
                .FirstOrDefaultAsync(
                    document =>
                        document.Id == documentId &&
                        document.UserId == userId
                );

        if (document == null)
        {
            return false;
        }

        if (
            document.ReviewStatus ==
            UserDocumentReviewStatus.Approved
        )
        {
            throw new InvalidOperationException(
                "Approved documents cannot be deleted."
            );
        }

        await _userFiles.DeleteStoredFileAsync(
            document.StoredFile
        );

        _dbContext.UserDocuments.Remove(
            document
        );

        await _dbContext.SaveChangesAsync();

        return true;
    }

    /* --------------------------------------------------------
     * UPLOAD RULES
     * -------------------------------------------------------- */

    private async Task ValidateUploadRulesAsync(
        Guid userId,
        UserDocumentType documentType
    )
    {
        if (
            documentType ==
            UserDocumentType.Identity
        )
        {
            var alreadyHasIdentity =
                await _dbContext.UserDocuments
                    .AnyAsync(
                        document =>
                            document.UserId == userId &&
                            document.DocumentType ==
                                UserDocumentType.Identity &&
                            document.StoredFile.Status !=
                                StoredFileStatus.Deleted &&
                            document.StoredFile.Status !=
                                StoredFileStatus.Failed
                    );

            if (alreadyHasIdentity)
            {
                throw new InvalidOperationException(
                    "An identity document has already been uploaded."
                );
            }

            return;
        }

        var professionalDocumentCount =
            await _dbContext.UserDocuments
                .CountAsync(
                    document =>
                        document.UserId == userId &&
                        document.DocumentType !=
                            UserDocumentType.Identity &&
                        document.StoredFile.Status !=
                            StoredFileStatus.Deleted &&
                        document.StoredFile.Status !=
                            StoredFileStatus.Failed
                );

        if (
            professionalDocumentCount >=
            MaxProfessionalDocuments
        )
        {
            throw new InvalidOperationException(
                $"You can upload a maximum of {MaxProfessionalDocuments} professional documents."
            );
        }
    }

    /* --------------------------------------------------------
     * VALIDATION
     * -------------------------------------------------------- */

    private static void ValidateDocument(
        IFormFile file
    )
    {
        if (
            file == null ||
            file.Length == 0
        )
        {
            throw new ArgumentException(
                "No document was uploaded."
            );
        }

        if (
            file.Length >
            MaxDocumentSize
        )
        {
            throw new ArgumentException(
                "Documents cannot exceed 10 MB."
            );
        }

        if (
            !AllowedDocumentTypes.Contains(
                file.ContentType
            )
        )
        {
            throw new ArgumentException(
                "Documents must be PDF, JPEG, PNG or WebP."
            );
        }
    }

    /* --------------------------------------------------------
     * MAPPING
     * -------------------------------------------------------- */

    private static UserDocumentDto MapToDto(
        UserDocument document
    )
    {
        return new UserDocumentDto(
            document.Id,
            document.DocumentType,
            document.ReviewStatus,
            document.StoredFile.OriginalFileName,
            document.StoredFile.ContentType,
            document.StoredFile.SizeBytes,
            document.CreatedAt,
            document.ReviewedAt
        );
    }
}