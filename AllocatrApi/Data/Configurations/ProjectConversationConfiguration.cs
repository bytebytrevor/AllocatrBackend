using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Configure;

public class ProjectConversationConfiguration : IEntityTypeConfiguration<ProjectConversation>
{
    public void Configure(EntityTypeBuilder<ProjectConversation> b)
    {
        b.ToTable("ProjectConversations");
        b.HasKey(x => x.Id);
        b.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        b.Property(x => x.TaskTitle).HasMaxLength(200);
        b.Property(x => x.LastSequence).IsConcurrencyToken();
        b.Property(x => x.PinVersion).IsConcurrencyToken();
        b.HasIndex(x => new { x.ProjectId, x.UpdatedAt, x.Id });
        b.HasOne(x => x.Project).WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Task).WithMany().HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ConversationParticipantConfiguration : IEntityTypeConfiguration<ConversationParticipant>
{
    public void Configure(EntityTypeBuilder<ConversationParticipant> b)
    {
        b.ToTable("ConversationParticipants");
        b.HasKey(x => new { x.ConversationId, x.UserId });
        b.HasIndex(x => new { x.UserId, x.IsArchived });
        b.Property(x => x.IsPinned).HasDefaultValue(false);
        b.HasOne(x => x.Conversation).WithMany(x => x.Participants).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> b)
    {
        b.ToTable("ConversationMessages");
        b.HasKey(x => x.Id);
        b.Property(x => x.Body).HasMaxLength(10000).IsRequired();
        b.Property(x => x.Version).HasDefaultValue(0L).IsConcurrencyToken();
        b.Property(x => x.ForwardedFromName).HasMaxLength(256);
        b.Property(x => x.ForwardNote).HasMaxLength(10000);
        b.Property(x => x.RequestHash).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.ConversationId, x.Sequence }).IsUnique();
        b.HasIndex(x => new { x.ConversationId, x.PinnedAt });
        b.HasOne(x => x.PinnedBy).WithMany().HasForeignKey(x => x.PinnedById).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.SenderId, x.ClientRequestId }).IsUnique();
        b.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Sender).WithMany().HasForeignKey(x => x.SenderId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class ConversationAttachmentConfiguration : IEntityTypeConfiguration<ConversationAttachment>
{
    public void Configure(EntityTypeBuilder<ConversationAttachment> b)
    {
        b.ToTable("ConversationAttachments");
        b.HasKey(x => x.Id);
        b.HasIndex(x => x.StoredFileId);
        b.HasOne(x => x.Message).WithMany(x => x.Attachments).HasForeignKey(x => x.MessageId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StoredFile).WithMany().HasForeignKey(x => x.StoredFileId).OnDelete(DeleteBehavior.Restrict);
    }
}
