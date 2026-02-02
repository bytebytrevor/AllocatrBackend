using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> entity)
    {
        entity.HasKey(t => t.Id);

        entity.Property(t => t.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.HasMany(t => t.Comments)
            .WithOne(c => c.TaskItem)
            .HasForeignKey(c => c.TaskItemId);

        entity.HasIndex(t => t.ProjectId);
    }
}
