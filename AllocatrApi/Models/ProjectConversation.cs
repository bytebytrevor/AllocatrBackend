namespace AllocatrApi.Models;

// The legacy ProjectMessage model/table is intentionally left intact.
public class ProjectConversation
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public Guid CreatedById { get; set; }
    public AllocatrUser CreatedBy { get; set; } = null!;
    public string Subject { get; set; } = string.Empty;
    public bool IsProjectWide { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    // Optimistic concurrency prevents two replies receiving the same position.
    public long LastSequence { get; set; }
    // Retained only for compatibility with the previously supplied message-pin migration.
    public long PinVersion { get; set; }
    public Guid? TaskId { get; set; }
    public TaskItem? Task { get; set; }
    public string? TaskTitle { get; set; }
    public ICollection<ConversationParticipant> Participants { get; set; } = [];
    public ICollection<ConversationMessage> Messages { get; set; } = [];
}

public class ConversationParticipant
{
    public Guid ConversationId { get; set; }
    public ProjectConversation Conversation { get; set; } = null!;
    public Guid UserId { get; set; }
    public AllocatrUser User { get; set; } = null!;
    public long LastReadSequence { get; set; }
    public bool IsArchived { get; set; }
    public bool IsPinned { get; set; }
}

public class ConversationMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public ProjectConversation Conversation { get; set; } = null!;
    public Guid SenderId { get; set; }
    public AllocatrUser Sender { get; set; } = null!;
    public long Sequence { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedById { get; set; }
    public long Version { get; set; }
    // Snapshot attribution: forwarding never exposes the private source conversation ID.
    public string? ForwardedFromName { get; set; }
    public DateTime? ForwardedCreatedAt { get; set; }
    public string? ForwardNote { get; set; }
    public Guid ClientRequestId { get; set; }
    public string RequestHash { get; set; } = string.Empty;
    // Legacy individual-message pin columns; no longer exposed or used by Messaging.
    public DateTime? PinnedAt { get; set; }
    public Guid? PinnedById { get; set; }
    public AllocatrUser? PinnedBy { get; set; }
    public ICollection<ConversationAttachment> Attachments { get; set; } = [];
}

// A separate link prevents message files being exposed by existing ProjectFiles routes.
public class ConversationAttachment
{
    public Guid Id { get; set; }
    public Guid MessageId { get; set; }
    public ConversationMessage Message { get; set; } = null!;
    public Guid StoredFileId { get; set; }
    public StoredFile StoredFile { get; set; } = null!;
}
