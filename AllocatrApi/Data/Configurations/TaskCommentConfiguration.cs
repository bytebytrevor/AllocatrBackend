using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
{
    public void Configure(EntityTypeBuilder<TaskComment> builder)
    {
        builder.ToTable("TaskComments");
        builder.HasKey(comment => comment.Id);
        builder.Property(comment => comment.Comment).IsRequired().HasMaxLength(2000);
        builder.Property(comment => comment.CreatedAt).IsRequired();
        builder.Property(comment => comment.UpdatedAt).IsRequired(false);
        builder.HasOne(comment => comment.TaskItem).WithMany(task => task.Comments)
            .HasForeignKey(comment => comment.TaskItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(comment => comment.CreatedBy).WithMany(user => user.TaskComments)
            .HasForeignKey(comment => comment.CreatedById).HasPrincipalKey(user => user.Id)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(comment => comment.TaskItemId);
        builder.HasIndex(comment => comment.CreatedById);
        builder.HasIndex(comment => new { comment.CreatedById, comment.ClientRequestId })
            .IsUnique().HasFilter("\"ClientRequestId\" IS NOT NULL");
    }
}
