using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> entity)
    {
        entity.HasKey(p => p.Id);

        entity.Property(p => p.Id)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("gen_random_uuid()");

        entity.Property(p => p.ProjectCode)
            .IsRequired()
            .HasMaxLength(50);

        entity.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(200);

        entity.Property(p => p.Currency)
            .HasMaxLength(3);

        entity.HasIndex(p => p.ProjectCode)
            .IsUnique();

        entity.HasIndex(p => p.UserId);

        entity.HasMany(p => p.Tasks)
            .WithOne(t => t.Project)
            .HasForeignKey(t => t.ProjectId);

        entity.HasMany(p => p.Messages)
            .WithOne(m => m.Project)
            .HasForeignKey(m => m.ProjectId);
    }
}