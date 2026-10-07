using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Configure;

public class StoredFileConfiguration
    : IEntityTypeConfiguration<StoredFile>
{
    public void Configure(
        EntityTypeBuilder<StoredFile> builder
    )
    {
        builder.HasKey(file => file.Id);

        builder.Property(file => file.Bucket)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(file => file.StoragePath)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(file => file.OriginalFileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(file => file.ContentType)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(file => file.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(file => file.CreatedAt)
            .IsRequired();

        builder.HasIndex(file => file.StoragePath)
            .IsUnique();

        builder.HasIndex(file => file.UploadedByUserId);

        builder.HasOne(file => file.UploadedByUser)
            .WithMany()
            .HasForeignKey(file => file.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}