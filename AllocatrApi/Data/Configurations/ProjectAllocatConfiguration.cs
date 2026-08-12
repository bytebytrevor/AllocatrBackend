using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectAllocatConfiguration
    : IEntityTypeConfiguration<ProjectAllocat>
{
    public void Configure(
        EntityTypeBuilder<ProjectAllocat> builder)
    {
        // Composite primary key
        builder.HasKey(pa => new
        {
            pa.ProjectId,
            pa.AllocatProfileId
        });

        // Project relationship
        builder.HasOne(pa => pa.Project)
            .WithMany(p => p.AllocatAssignments)
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Allocat profile relationship
        builder.HasOne(pa => pa.AllocatProfile)
            .WithMany(a => a.ProjectAssignments)
            .HasForeignKey(pa => pa.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        // Status
        builder.Property(pa => pa.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Invitation timestamp
        builder.Property(pa => pa.InvitedAt)
            .IsRequired();

        // Nullable lifecycle timestamps
        builder.Property(pa => pa.RespondedAt)
            .IsRequired(false);

        builder.Property(pa => pa.RemovedAt)
            .IsRequired(false);

        // Useful indexes for common queries
        builder.HasIndex(pa => pa.ProjectId);

        builder.HasIndex(pa => pa.AllocatProfileId);

        builder.HasIndex(pa => pa.Status);

        builder.HasIndex(pa => new
        {
            pa.AllocatProfileId,
            pa.Status
        });

        builder.HasIndex(pa => new
        {
            pa.ProjectId,
            pa.Status
        });
    }
}