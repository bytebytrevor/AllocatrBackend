using Microsoft.AspNetCore.Http;

namespace AllocatrApi.Dtos;

public record ConversationMemberDto(Guid UserId, string Name, string? AvatarUrl, bool IsOwner);
public record ConversationSummaryDto(Guid Id, string Subject, bool IsProjectWide,
    DateTime UpdatedAt, string Preview, int UnreadCount, long LastSequence, bool IsArchived,
    Guid? TaskId, string? TaskTitle, bool IsPinned, bool HasAttachments);
public record ConversationListDto(IReadOnlyList<ConversationSummaryDto> Items, int Page, int TotalCount, int UnreadCount);
public record ConversationFileDto(Guid Id, string OriginalFileName, string ContentType, long SizeBytes);
public record ConversationMessageDto(Guid Id, long Sequence, Guid SenderId, string SenderName,
    string? AvatarUrl, string Body, DateTime CreatedAt, IReadOnlyList<ConversationFileDto> Attachments,
    DateTime? EditedAt, DateTime? DeletedAt, long Version, DateTime ActionExpiresAt,
    bool CanEdit, bool CanDelete, bool CanModerate, string? ForwardedFromName, DateTime? ForwardedCreatedAt, string? ForwardNote);
public record ConversationDetailDto(Guid Id, string Subject, bool IsProjectWide, bool IsArchived,
    Guid? TaskId, string? TaskTitle, IReadOnlyList<ConversationMemberDto> Participants,
    IReadOnlyList<ConversationMessageDto> Messages, bool HasOlder, long LastSequence, bool IsPinned);
public record ConversationSendResultDto(Guid ConversationId, Guid MessageId);
public record ConversationReadDto(long ThroughSequence);
public record ConversationArchiveDto(bool Archived);

public class CreateConversationForm
{
    public string Subject { get; set; } = string.Empty;
    public bool IsProjectWide { get; set; }
    public List<Guid> RecipientIds { get; set; } = [];
    public string Body { get; set; } = string.Empty;
    public Guid ClientRequestId { get; set; }
    public Guid? TaskId { get; set; }
    public List<IFormFile> Files { get; set; } = [];
}
public class ReplyConversationForm
{
    public string Body { get; set; } = string.Empty;
    public Guid ClientRequestId { get; set; }
    public List<IFormFile> Files { get; set; } = [];
}

public record ConversationPinDto(bool Pinned);

public record ConversationEditDto(string Body, long Version);
public record ConversationDeleteDto(long Version);
public class ConversationForwardDto
{
    public Guid ClientRequestId { get; set; }
    public long Version { get; set; }
    public string Subject { get; set; } = string.Empty;
    public bool IsProjectWide { get; set; }
    public List<Guid> RecipientIds { get; set; } = [];
    public string? Note { get; set; }
}
