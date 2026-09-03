using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
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
            .IsRequired();

        entity.Property(p => p.Priority);

        entity.Property(p => p.Progress)
            .IsRequired();

        /* =================================================
           DATES
        ================================================= */

        entity.Property(p => p.CreatedAt)
            .IsRequired();

        entity.Property(p => p.UpdatedAt);

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