using AllocatrApi.Enums;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectAllocatConfiguration : IEntityTypeConfiguration<ProjectAllocat>
{
    public void Configure(EntityTypeBuilder<ProjectAllocat> builder)
    {
        builder.HasKey(pa => new { pa.ProjectId, pa.AllocatProfileId });
        builder
            .HasOne(pa => pa.Project)
            .WithMany(p => p.AllocatAssignments)
            .HasForeignKey(pa => pa.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder
            .HasOne(pa => pa.AllocatProfile)
            .WithMany(a => a.ProjectAssignments)
            .HasForeignKey(pa => pa.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Property(pa => pa.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(pa => pa.InvitedAt).IsRequired();
        builder.Property(pa => pa.RespondedAt).IsRequired(false);
        builder.Property(pa => pa.RemovedAt).IsRequired(false);
        builder.Property(pa => pa.LeftAt).IsRequired(false);
        builder.Property(pa => pa.LeaveReason).HasMaxLength(1000);
        builder.HasIndex(pa => pa.ProjectId);
        builder.HasIndex(pa => pa.AllocatProfileId);
        builder.HasIndex(pa => pa.Status);
        builder.HasIndex(pa => new { pa.AllocatProfileId, pa.Status });
        builder.HasIndex(pa => new { pa.ProjectId, pa.Status });
    }
}
