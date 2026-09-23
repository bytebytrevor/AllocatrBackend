using AllocatrApi.Constants;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.ToTable(
            "Projects",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Projects_Status",
                    "\"Status\" IN ('pending', 'active', 'completion_requested', 'completed')"
                );

                table.HasCheckConstraint(
                    "CK_Projects_Progress",
                    "\"Progress\" >= 0 AND \"Progress\" <= 100"
                );
            }
        );

        /* =================================================
           KEY
        ================================================= */

        entity.HasKey(p => p.Id);

        entity.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        /* =================================================
           PROJECT INFORMATION
        ================================================= */

        entity.Property(p => p.ProjectCode)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(2000);

        entity.Property(p => p.Category)
            .IsRequired();

        /* =================================================
           PROJECT STATE
        ================================================= */

        entity.Property(p => p.Status)
            .HasMaxLength(30)
            .IsRequired()
            .HasDefaultValue(ProjectStatuses.Pending);

        entity.Property(p => p.Priority);

        entity.Property(p => p.Progress)
            .IsRequired()
            .HasDefaultValue(0);

        /* =================================================
           DATES
        ================================================= */

        entity.Property(p => p.CreatedAt)
            .IsRequired();

        entity.Property(p => p.UpdatedAt);
        entity.Property(p => p.CompletionRequestedAt);
        entity.Property(p => p.CompletedAt);
        entity.Property(p => p.StartDate);
        entity.Property(p => p.DueDate);

        /* =================================================
           FINANCIAL
        ================================================= */

        entity.Property(p => p.Budget);

        entity.Property(p => p.Currency)
            .IsRequired()
            .HasMaxLength(3);

        /* =================================================
           PROJECT OPTIONS
        ================================================= */

        entity.Property(p => p.IsPublic)
            .IsRequired();

        entity.Property(p => p.AllowBids)
            .IsRequired();

        /* =================================================
           OWNER
        ================================================= */

        entity.Property(p => p.UserId)
            .IsRequired();

        entity.HasIndex(p => p.UserId);

        entity.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        /* =================================================
           COMPLETION REQUESTER
        ================================================= */

        entity.HasOne(p => p.CompletionRequestedByAllocat)
            .WithMany()
            .HasForeignKey(p => p.CompletionRequestedByAllocatId)
            .OnDelete(DeleteBehavior.SetNull);

        /* =================================================
           INDEXES
        ================================================= */

        entity.HasIndex(p => p.ProjectCode)
            .IsUnique();

        /* =================================================
           TASKS
        ================================================= */

        entity.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        /* =================================================
           MESSAGES
        ================================================= */

        entity.HasMany(p => p.Messages)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}