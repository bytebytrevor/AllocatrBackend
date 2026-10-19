namespace AllocatrApi.Dtos;

public record ProjectFileDto(
    Guid Id,
    Guid ProjectId,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    Guid UploadedByUserId,
    string UploadedByName,
    DateTime CreatedAt,
    string Context,
    Guid? TaskId,
    string? TaskTitle,
    bool CanDelete,
    Guid? CommentId = null);

public record ProjectFilePageDto(
    IReadOnlyList<ProjectFileDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool CanUpload = false);

public record ProjectFileDownloadDto(string Url, DateTime ExpiresAt);
