using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ReviewConfiguration :
    IEntityTypeConfiguration<Review>
{
    public void Configure(
        EntityTypeBuilder<Review> builder)
    {
        builder.ToTable(
            "Reviews",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_Reviews_Rating",
                    "\"Rating\" >= 1 AND \"Rating\" <= 5"
                );
            }
        );

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        /*
         * One Allocat can only have one review
         * for a specific project.
         */
        builder.HasIndex(r => new
        {
            r.ProjectId,
            r.AllocatProfileId
        })
        .IsUnique();

        builder.HasIndex(r => r.AllocatProfileId);

        builder.HasOne(r => r.Project)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AllocatProfile)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}