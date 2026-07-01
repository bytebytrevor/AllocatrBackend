using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Rating)
            .HasPrecision(2, 1);

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(r => new
        {
            r.ProjectId,
            r.ReviewerId,
            r.AllocatProfileId
        }).IsUnique();

        builder.HasOne(r => r.Project)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.ProjectId);

        builder.HasOne(r => r.Reviewer)
            .WithMany(u => u.ReviewsWritten)
            .HasForeignKey(r => r.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AllocatProfile)
            .WithMany(a => a.Reviews)
            .HasForeignKey(r => r.AllocatProfileId);


    }
}