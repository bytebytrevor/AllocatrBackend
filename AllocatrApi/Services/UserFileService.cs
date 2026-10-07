using AllocatrApi.Data;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;

namespace AllocatrApi.Services;

public class UserFileService
{
    public const string AvatarBucket = "avatars";
    public const string UserDocumentBucket = "user-documents";

    private readonly AllocatrDbContext _dbContext;
    private readonly IFileStorageService _storage;

    public UserFileService(
        AllocatrDbContext dbContext,
        IFileStorageService storage
    )
    {
        _dbContext = dbContext;
        _storage = storage;
    }

    /* --------------------------------------------------------
     * AVATAR
     * -------------------------------------------------------- */

    public async Task<StoredFile> UploadAvatarAsync(
        Guid userId,
        IFormFile file
    )
    {
        var extension = GetFileExtension(
            file.ContentType
        );

        var fileId = Guid.NewGuid();

        var storagePath =
            $"users/{userId}/avatar/{fileId}{extension}";

        return await UploadStoredFileAsync(
            fileId,
            userId,
            AvatarBucket,
            storagePath,
            file
        );
    }

    /* --------------------------------------------------------
     * USER DOCUMENTS
     * -------------------------------------------------------- */

    public async Task<StoredFile> UploadUserDocumentAsync(
        Guid userId,
        UserDocumentType documentType,
        IFormFile file
    )
    {
        var extension = GetFileExtension(
            file.ContentType
        );

        var fileId = Guid.NewGuid();

        var folder =
            documentType == UserDocumentType.Identity
                ? "identity"
                : "credentials";

        var storagePath =
            $"users/{userId}/{folder}/{fileId}{extension}";

        return await UploadStoredFileAsync(
            fileId,
            userId,
            UserDocumentBucket,
            storagePath,
            file
        );
    }

    /* --------------------------------------------------------
     * DELETE
     * -------------------------------------------------------- */

    public async Task DeleteStoredFileAsync(
        StoredFile storedFile
    )
    {
        if (storedFile.Status == StoredFileStatus.Deleted)
        {
            return;
        }

        await _storage.DeleteAsync(
            storedFile.Bucket,
            storedFile.StoragePath
        );

        storedFile.Status =
            StoredFileStatus.Deleted;

        storedFile.DeletedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    /* --------------------------------------------------------
     * SHARED UPLOAD
     * -------------------------------------------------------- */

    private async Task<StoredFile> UploadStoredFileAsync(
        Guid fileId,
        Guid userId,
        string bucket,
        string storagePath,
        IFormFile file
    )
    {
        var storedFile = new StoredFile
        {
            Id = fileId,
            UploadedByUserId = userId,
            Bucket = bucket,
            StoragePath = storagePath,
            OriginalFileName = CleanFileName(
                file.FileName
            ),
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            Status = StoredFileStatus.Uploading,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.StoredFiles.Add(
            storedFile
        );

        await _dbContext.SaveChangesAsync();

        try
        {
            var bytes =
                await ReadBytesAsync(file);

            await _storage.UploadAsync(
                bucket,
                storagePath,
                bytes
            );

            storedFile.Status =
                StoredFileStatus.Ready;

            storedFile.ReadyAt =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return storedFile;
        }
        catch
        {
            storedFile.Status =
                StoredFileStatus.Failed;

            await _dbContext.SaveChangesAsync();

            throw;
        }
    }

    /* --------------------------------------------------------
     * HELPERS
     * -------------------------------------------------------- */

    private static async Task<byte[]> ReadBytesAsync(
        IFormFile file
    )
    {
        await using var stream =
            new MemoryStream();

        await file.CopyToAsync(stream);

        return stream.ToArray();
    }

    private static string CleanFileName(
        string fileName
    )
    {
        var name = Path.GetFileName(
            fileName
        );

        if (string.IsNullOrWhiteSpace(name))
        {
            return "file";
        }

        return name.Length <= 255
            ? name
            : name[..255];
    }

    private static string GetFileExtension(
        string contentType
    )
    {
        return contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            "application/pdf" => ".pdf",

            _ => throw new ArgumentException(
                "Unsupported file type."
            ),
        };
    }
}