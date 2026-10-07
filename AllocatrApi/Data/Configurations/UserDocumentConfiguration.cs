using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Configure;

public class UserDocumentConfiguration
    : IEntityTypeConfiguration<UserDocument>
{
    public void Configure(
        EntityTypeBuilder<UserDocument> builder
    )
    {
        builder.HasKey(document => document.Id);

        builder.Property(document => document.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(document => document.ReviewStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(document => document.ReviewNotes)
            .HasMaxLength(1000);

        builder.Property(document => document.CreatedAt)
            .IsRequired();

        builder.HasIndex(document => document.UserId);

        builder.HasIndex(document => document.StoredFileId)
            .IsUnique();

        builder.HasOne(document => document.User)
            .WithMany()
            .HasForeignKey(document => document.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(document => document.StoredFile)
            .WithOne()
            .HasForeignKey<UserDocument>(
                document => document.StoredFileId
            )
            .OnDelete(DeleteBehavior.Restrict);
    }
}