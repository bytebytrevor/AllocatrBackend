using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Configure;

public class ProjectFileConfiguration : IEntityTypeConfiguration<ProjectFile>
{
    public void Configure(EntityTypeBuilder<ProjectFile> builder)
    {
        builder.HasKey(file => file.Id);
        builder.Property(file => file.Context).HasConversion<string>()
            .HasMaxLength(30).IsRequired();
        builder.Property(file => file.TaskTitle).HasMaxLength(200);
        builder.Property(file => file.CreatedAt).IsRequired();
        builder.HasIndex(file => file.StoredFileId).IsUnique();
        builder.HasIndex(file => new { file.ProjectId, file.CreatedAt });
        builder.HasIndex(file => new { file.ProjectId, file.TaskId });
        builder.HasIndex(file => file.UploadedByUserId);
        // Do not cascade-delete metadata while private bytes still exist.
        builder.HasOne(file => file.Project).WithMany()
            .HasForeignKey(file => file.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(file => file.StoredFile).WithOne()
            .HasForeignKey<ProjectFile>(file => file.StoredFileId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(file => file.CommentId);
        builder.HasOne(file => file.Comment).WithMany()
            .HasForeignKey(file => file.CommentId).OnDelete(DeleteBehavior.SetNull);
        // Deleting a task retains its files in the project's central Files list.
        builder.HasOne(file => file.Task).WithMany()
            .HasForeignKey(file => file.TaskId).OnDelete(DeleteBehavior.SetNull);
    }
}
