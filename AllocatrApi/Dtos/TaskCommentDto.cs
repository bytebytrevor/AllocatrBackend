namespace AllocatrApi.Dtos;

public record class TaskCommentDto(
    Guid Id,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string Comment,
    Guid CreatedById,
    string CreatedByName,
    string? AvatarUrl,
    Guid TaskItemId,
    IReadOnlyList<ProjectFileDto>? Attachments = null);

public record TaskDiscussionContextDto(
    Guid ProjectId,
    bool CanUploadFiles,
    int MaxAttachments,
    long MaxFileSizeBytes);
