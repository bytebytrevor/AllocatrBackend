using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> entity)
    {
        entity.ToTable("TaskItems");

        entity.HasKey(t => t.Id);

        entity.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        // Project relationship
        entity.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Assigned user
        entity.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // Created by user
        entity.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Comments
        entity.HasMany(t => t.Comments)
            .WithOne(c => c.TaskItem)
            .HasForeignKey(c => c.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        entity.HasIndex(t => t.ProjectId);
        entity.HasIndex(t => t.AssignedToId);
        entity.HasIndex(t => t.CreatedByUserId);
    }
}
