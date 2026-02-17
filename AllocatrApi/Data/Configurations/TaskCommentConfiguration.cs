using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Configurations;

public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        // Table name
        builder.ToTable("TaskComments");

        // Primary Key
        builder.HasKey(tc => tc.Id);

        // Properties
        builder.Property(tc => tc.Comment)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(tc => tc.CreatedAt)
            .IsRequired();

        builder.Property(tc => tc.UpdatedAt)
            .IsRequired(false);

        // Task relationship (many comments → one task)
        builder.HasOne(tc => tc.TaskItem)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // User relationship (many comments → one user)
        builder.HasOne(tc => tc.CreatedBy)
            .WithMany(u => u.TaskComments)
            .HasForeignKey(tc => tc.CreatedById)
            .HasPrincipalKey(u => u.Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes (performance)
        builder.HasIndex(tc => tc.TaskItemId);
        builder.HasIndex(tc => tc.CreatedById);
    }
}
