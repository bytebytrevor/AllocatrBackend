using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> b)
    {
        b.ToTable("UserNotifications");
        b.HasKey(n => n.Id);
        b.Property(n => n.Id).ValueGeneratedOnAdd();
        b.Property(n => n.Kind).HasMaxLength(40).IsRequired();
        b.Property(n => n.ProjectTitle).HasMaxLength(160).IsRequired();
        b.Property(n => n.Text).HasMaxLength(500).IsRequired();
        b.Property(n => n.Href).HasMaxLength(400).IsRequired();
        b.Property(n => n.ItemCount).HasDefaultValue(1);
        b.HasOne<AllocatrUser>()
            .WithMany()
            .HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(n => new { n.RecipientId, n.Id });
        b.HasIndex(n => new { n.RecipientId, n.ReadAt });
        b.HasIndex(n => new { n.RecipientId, n.EventId }).IsUnique();
    }
}
