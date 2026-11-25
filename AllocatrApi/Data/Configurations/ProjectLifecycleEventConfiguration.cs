using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public sealed class ProjectLifecycleEventConfiguration
    : IEntityTypeConfiguration<ProjectLifecycleEvent>
{
    public void Configure(EntityTypeBuilder<ProjectLifecycleEvent> b)
    {
        b.ToTable("ProjectLifecycleEvents");
        b.HasKey(e => e.Id);
        b.Property(e => e.Kind).IsRequired().HasMaxLength(40);
        b.Property(e => e.Reason).IsRequired().HasMaxLength(1000);
        b.Property(e => e.ProjectTitle).IsRequired().HasMaxLength(200);
        b.Property(e => e.ProjectCode).IsRequired().HasMaxLength(50);
        b.Property(e => e.ActorName).IsRequired();
        b.Property(e => e.RecipientIds).HasColumnType("uuid[]").IsRequired();
        b.HasIndex(e => new { e.ProjectId, e.CreatedAt });
        b.HasOne<Project>()
            .WithMany()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
