using AllocatrApi.Constants;
using AllocatrApi.Data;
using AllocatrApi.Dtos;
using AllocatrApi.Models;
using AllocatrApi.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AllocatrApi.Services;

public class ProjectFileService
{
    public const string Bucket = "project-files";
    public const int DownloadLifetimeSeconds = 300;
    private readonly AllocatrDbContext _db;
    private readonly ProjectAccessService _access;
    private readonly IFileStorageService _storage;
    private readonly ProjectFileValidationService _validation;
    private readonly ILogger<ProjectFileService> _logger;

    public ProjectFileService(AllocatrDbContext db, ProjectAccessService access,
        IFileStorageService storage, ProjectFileValidationService validation,
        ILogger<ProjectFileService> logger)
    {
        _db = db;
        _access = access;
        _storage = storage;
        _validation = validation;
        _logger = logger;
    }

    public async Task<ProjectFilePageDto> ListAsync(Guid projectId, Guid userId,
        bool isAllocat, Guid? taskId = null, int page = 1, int pageSize = 50,
        CancellationToken cancellationToken = default, string? search = null, string? source = null, string? sort = null, string? kind = null)
    {
        var project = await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
        if (taskId.HasValue) await RequireTaskAsync(projectId, taskId.Value, cancellationToken);
        page = Math.Clamp(page, 1, 1_000_000);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var query = _db.ProjectFiles.AsNoTracking().Where(file =>
            file.ProjectId == projectId && file.StoredFile.Status == StoredFileStatus.Ready);
        if (taskId.HasValue) query = query.Where(file => file.TaskId == taskId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            if (term.Length > 100) throw new ProjectFileException(400, "Search cannot exceed 100 characters.");
            query = query.Where(file => file.StoredFile.OriginalFileName.ToLower().Contains(term)
                || file.StoredFile.UploadedByUser.FullName.ToLower().Contains(term)
                || (file.Task != null && file.Task.Title.ToLower().Contains(term))
                || (file.TaskTitle != null && file.TaskTitle.ToLower().Contains(term)));
        }
        if (!string.IsNullOrWhiteSpace(source))
        {
            var context = source.Trim().ToLowerInvariant() switch
            {
                "project" => ProjectFileContext.Project,
                "task" => ProjectFileContext.Task,
                "comment" => ProjectFileContext.Comment,
                _ => throw new ProjectFileException(400, "Invalid file source filter."),
            };
            query = query.Where(file => file.Context == context);
        }
        var category = kind?.Trim().ToLowerInvariant() ?? "all";
        query = category switch
        {
            "all" => query,
            "images" => query.Where(file => file.StoredFile.ContentType == "image/jpeg" || file.StoredFile.ContentType == "image/png" || file.StoredFile.ContentType == "image/webp"),
            "video" => query.Where(file => file.StoredFile.ContentType == "video/mp4" || file.StoredFile.ContentType == "video/webm"),
            "documents" => query.Where(file => file.StoredFile.ContentType == "application/pdf" || file.StoredFile.ContentType == "application/msword" || file.StoredFile.ContentType == "application/vnd.openxmlformats-officedocument.wordprocessingml.document" || file.StoredFile.ContentType == "application/vnd.ms-excel" || file.StoredFile.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" || file.StoredFile.ContentType == "text/plain" || file.StoredFile.ContentType == "text/csv" || file.StoredFile.ContentType == "application/csv"),
            _ => throw new ProjectFileException(400, "Invalid file type filter."),
        };
        var count = await query.CountAsync(cancellationToken);
        var owner = project.UserId == userId;
        var writable = IsWritable(project.Status);
        // Sort before paging so all pages follow the selected order.
        var ordered = (sort?.Trim().ToLowerInvariant() ?? "newest") switch
        {
            "newest" => query.OrderByDescending(file => file.CreatedAt).ThenByDescending(file => file.Id),
            "oldest" => query.OrderBy(file => file.CreatedAt).ThenBy(file => file.Id),
            "name-asc" => query.OrderBy(file => file.StoredFile.OriginalFileName.ToLower()).ThenBy(file => file.Id),
            "name-desc" => query.OrderByDescending(file => file.StoredFile.OriginalFileName.ToLower()).ThenBy(file => file.Id),
            "size-desc" => query.OrderByDescending(file => file.StoredFile.SizeBytes).ThenBy(file => file.Id),
            "type-asc" => query.OrderBy(file => file.StoredFile.ContentType).ThenBy(file => file.StoredFile.OriginalFileName.ToLower()).ThenBy(file => file.Id),
            "type-desc" => query.OrderByDescending(file => file.StoredFile.ContentType).ThenBy(file => file.StoredFile.OriginalFileName.ToLower()).ThenBy(file => file.Id),
            "size-asc" => query.OrderBy(file => file.StoredFile.SizeBytes).ThenBy(file => file.Id),
            _ => throw new ProjectFileException(400, "Invalid file sort option."),
        };
        var items = await ordered.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(file => new ProjectFileDto(
                file.Id, file.ProjectId, file.StoredFile.OriginalFileName,
                file.StoredFile.ContentType, file.StoredFile.SizeBytes,
                file.UploadedByUserId, file.StoredFile.UploadedByUser.FullName,
                file.CreatedAt, file.Context == ProjectFileContext.Comment ? "comment" : file.Context == ProjectFileContext.Task ? "task" : "project",
                file.TaskId, file.Task != null ? file.Task.Title : file.TaskTitle,
                writable && (owner || file.UploadedByUserId == userId), file.CommentId))
            .ToListAsync(cancellationToken);
        return new ProjectFilePageDto(items, page, pageSize, count, writable);
    }

    public async Task<ProjectFileDto> UploadAsync(Guid projectId, Guid userId,
        bool isAllocat, Guid? taskId, IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        var project = await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
        EnsureWritable(project.Status);
        var taskTitle = taskId.HasValue
            ? await RequireTaskAsync(projectId, taskId.Value, cancellationToken) : null;
        var uploader = await _db.Users.AsNoTracking().Where(user => user.Id == userId)
            .Select(user => user.FullName).FirstOrDefaultAsync(cancellationToken) ?? "Project member";
        var upload = await _validation.ValidateAndReadAsync(file, cancellationToken);
        var now = DateTime.UtcNow;
        var stored = new StoredFile
        {
            Id = Guid.NewGuid(),
            UploadedByUserId = userId,
            Bucket = Bucket,
            OriginalFileName = upload.OriginalFileName,
            ContentType = upload.ContentType,
            SizeBytes = upload.Bytes.LongLength,
            Status = StoredFileStatus.Uploading,
            CreatedAt = now,
        };
        stored.StoragePath = $"projects/{projectId}/files/{stored.Id}{upload.Extension}";
        var attachment = new ProjectFile
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            TaskId = taskId,
            TaskTitle = taskTitle,
            StoredFileId = stored.Id,
            StoredFile = stored,
            UploadedByUserId = userId,
            Context = taskId.HasValue ? ProjectFileContext.Task : ProjectFileContext.Project,
            CreatedAt = now,
        };
        _db.StoredFiles.Add(stored);
        _db.ProjectFiles.Add(attachment);
        // One SaveChanges transaction creates both pending records before uploading.
        // If this fails, no storage upload has occurred.
        await _db.SaveChangesAsync(cancellationToken);

        try
        {
            await _storage.UploadAsync(Bucket, stored.StoragePath, upload.Bytes, upsert: false);
            // Recheck after the potentially slow remote upload, before publishing it.
            project = await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
            EnsureWritable(project.Status);
            if (taskId.HasValue) await RequireTaskAsync(projectId, taskId.Value, cancellationToken);
            stored.Status = StoredFileStatus.Ready;
            stored.ReadyAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // A storage timeout can still have created bytes. The unique path makes
            // compensating deletion safe even if the SDK did not return success.
            stored.Status = StoredFileStatus.Failed;
            stored.ReadyAt = null;
            try { await _db.SaveChangesAsync(CancellationToken.None); }
            catch (Exception metadataError)
            {
                _logger.LogError(metadataError,
                    "Could not mark failed project upload {StoredFileId}", stored.Id);
            }
            try
            {
                await _storage.DeleteAsync(Bucket, stored.StoragePath);
                attachment.StorageDeletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(CancellationToken.None);
            }
            catch (Exception cleanupError)
            {
                _logger.LogError(cleanupError,
                    "Project upload cleanup required for {StoredFileId}", stored.Id);
            }
            throw;
        }
        return new ProjectFileDto(attachment.Id, projectId, upload.OriginalFileName,
            upload.ContentType, upload.Bytes.LongLength, userId, uploader, now,
            taskId.HasValue ? "task" : "project", taskId, taskTitle, true);
    }

    public async Task<ProjectFileDownloadDto> GetDownloadAsync(Guid projectId,
        Guid fileId, Guid userId, bool isAllocat,
        CancellationToken cancellationToken = default)
    {
        await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
        var file = await _db.ProjectFiles.AsNoTracking()
            .Where(file => file.Id == fileId && file.ProjectId == projectId
                && file.StoredFile.Status == StoredFileStatus.Ready)
            .Select(file => new { file.StoredFile.Bucket, file.StoredFile.StoragePath })
            .FirstOrDefaultAsync(cancellationToken);
        if (file == null) throw new ProjectFileException(404, "File not found.");
        // Stamp before signing so the advertised lifetime is conservative.
        var expiresAt = DateTime.UtcNow.AddSeconds(DownloadLifetimeSeconds);
        try
        {
            var url = await _storage.CreateSignedUrlAsync(file.Bucket,
                file.StoragePath, DownloadLifetimeSeconds);
            if (string.IsNullOrWhiteSpace(url)) throw new IOException("Empty storage URL.");
            return new ProjectFileDownloadDto(url, expiresAt);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not sign project file {ProjectFileId}", fileId);
            throw new ProjectFileException(503, "The file download is temporarily unavailable. Please try again.");
        }
    }

    public async Task<ProjectThumbnailSource> GetThumbnailSourceAsync(Guid projectId,
        Guid fileId, Guid userId, bool isAllocat, CancellationToken cancellationToken = default)
    {
        await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
        var file = await _db.ProjectFiles.AsNoTracking()
            .Where(file => file.Id == fileId && file.ProjectId == projectId && file.StoredFile.Status == StoredFileStatus.Ready)
            .Select(file => new ProjectThumbnailSource(file.StoredFile.Bucket, file.StoredFile.StoragePath, file.StoredFile.ContentType))
            .FirstOrDefaultAsync(cancellationToken);
        if (file == null) throw new ProjectFileException(404, "File not found.");
        if (file.ContentType is not ("image/jpeg" or "image/png" or "image/webp" or "video/mp4" or "video/webm"))
            throw new ProjectFileException(415, "This file does not have a thumbnail.");
        return file;
    }

    public async Task DeleteAsync(Guid projectId, Guid fileId, Guid userId,
        bool isAllocat, CancellationToken cancellationToken = default)
    {
        var project = await RequireAccessAsync(projectId, userId, isAllocat, cancellationToken);
        var file = await _db.ProjectFiles.Include(file => file.StoredFile)
            .FirstOrDefaultAsync(file => file.Id == fileId && file.ProjectId == projectId,
                cancellationToken);
        if (file == null) throw new ProjectFileException(404, "File not found.");
        if (project.UserId != userId && file.UploadedByUserId != userId)
            throw new ProjectFileException(403, "Only the project owner or the uploader can delete this file.");
        if (file.StoredFile.Status != StoredFileStatus.Deleted)
        {
            EnsureWritable(project.Status);
            if (file.StoredFile.Status != StoredFileStatus.Ready)
                throw new ProjectFileException(409, "Only a ready file can be deleted.");
            file.StoredFile.Status = StoredFileStatus.Deleted;
            file.StoredFile.DeletedAt = DateTime.UtcNow;
            // Hide from all future lists and download requests before remote deletion.
            await _db.SaveChangesAsync(cancellationToken);
        }
        if (file.StorageDeletedAt.HasValue) return;
        try
        {
            // A retry can finish pending byte removal even if the project has since
            // completed. It never restores a logically deleted file.
            await _storage.DeleteAsync(file.StoredFile.Bucket, file.StoredFile.StoragePath);
            file.StorageDeletedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Storage deletion pending for project file {ProjectFileId}", fileId);
            throw new ProjectFileException(503,
                "The file has been hidden, but storage cleanup is pending. Retry deleting it to finish cleanup.");
        }
    }

    private async Task<ProjectState> RequireAccessAsync(Guid projectId, Guid userId,
        bool isAllocat, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.AsNoTracking().Where(project => project.Id == projectId)
            .Select(project => new ProjectState(project.UserId, project.Status))
            .FirstOrDefaultAsync(cancellationToken);
        // Private collaboration access: public project visibility does not grant access.
        if (userId == Guid.Empty || project == null
            || (project.UserId != userId && (!isAllocat
                || !await _access.IsAcceptedAllocatAsync(projectId, userId))))
            throw new ProjectFileException(404, "Project not found or not accessible.");
        return project;
    }

    private async Task<string> RequireTaskAsync(Guid projectId, Guid taskId,
        CancellationToken cancellationToken)
    {
        var task = await _db.TaskItems.AsNoTracking()
            .Where(task => task.Id == taskId && task.ProjectId == projectId)
            .Select(task => new { task.Title }).FirstOrDefaultAsync(cancellationToken);
        if (task == null) throw new ProjectFileException(404, "Task not found in this project.");
        return task.Title;
    }

    private static bool IsWritable(string status) =>
        status != ProjectStatuses.CompletionRequested && status != ProjectStatuses.Completed;

    private static void EnsureWritable(string status)
    {
        if (!IsWritable(status)) throw new ProjectFileException(409,
            "Files cannot be added or deleted while completion is awaiting confirmation or the project is completed.");
    }

    private sealed record ProjectState(Guid UserId, string Status);
}
