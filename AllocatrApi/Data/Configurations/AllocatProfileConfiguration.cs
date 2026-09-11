using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class AllocatProfileConfiguration : IEntityTypeConfiguration<AllocatProfile>
{
    public void Configure(EntityTypeBuilder<AllocatProfile> builder)
    {
        builder.ToTable(
            "AllocatProfiles",
            table =>
            {
                table.HasCheckConstraint(
                    "CK_AllocatProfiles_HourlyRate",
                    "\"HourlyRate\" IS NULL OR \"HourlyRate\" >= 0"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_YearsExperience",
                    "\"YearsExperience\" IS NULL OR " +
                    "(\"YearsExperience\" >= 0 AND \"YearsExperience\" <= 80)"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_AverageRating",
                    "\"AverageRating\" >= 0 AND \"AverageRating\" <= 5"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_RatingCount",
                    "\"RatingCount\" >= 0"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_Availability",
                    "\"Availability\" IN ('available', 'busy', 'unavailable')"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_Level",
                    "\"Level\" >= 1 AND \"Level\" <= 5"
                );

                table.HasCheckConstraint(
                    "CK_AllocatProfiles_ResponseTime",
                    "\"AverageResponseTimeMinutes\" IS NULL OR " +
                    "\"AverageResponseTimeMinutes\" >= 0"
                );
            }
        );

        builder.HasKey(a => a.AllocatrUserId);

        builder.Property(a => a.IdNumber)
            .HasMaxLength(50);

        builder.Property(a => a.Title)
            .HasMaxLength(120);

        builder.Property(a => a.Headline)
            .HasMaxLength(180);

        builder.Property(a => a.Bio)
            .HasMaxLength(500);

        builder.Property(a => a.HourlyRate)
            .HasPrecision(12, 2);

        builder.Property(a => a.Currency)
            .HasMaxLength(3)
            .IsRequired()
            .HasDefaultValue("USD");

        builder.Property(a => a.Availability)
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue("available");

        builder.Property(a => a.AverageRating)
            .HasPrecision(3, 2)
            .HasDefaultValue(0m);

        builder.Property(a => a.RatingCount)
            .HasDefaultValue(0);

        builder.Property(a => a.IsVerified)
            .HasDefaultValue(false);

        builder.Property(a => a.Level)
            .HasDefaultValue(1);

        builder.Property(a => a.IsVisible)
            .HasDefaultValue(true);

        builder.Property(a => a.CreatedAt)
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .IsRequired();

        builder.HasIndex(a => a.IdNumber)
            .IsUnique()
            .HasFilter("\"IdNumber\" IS NOT NULL");

        builder.HasIndex(a => a.IsVisible);

        builder.HasIndex(a => a.Availability);

        builder.HasIndex(a => new
        {
            a.IsVisible,
            a.Availability
        });

        builder.HasIndex(a => a.HourlyRate);

        builder.HasOne(a => a.AllocatrUser)
            .WithOne(u => u.AllocatProfile)
            .HasForeignKey<AllocatProfile>(a => a.AllocatrUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Skills)
            .WithOne(s => s.AllocatProfile)
            .HasForeignKey(s => s.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Reviews)
            .WithOne(r => r.AllocatProfile)
            .HasForeignKey(r => r.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.ProjectAssignments)
            .WithOne(pa => pa.AllocatProfile)
            .HasForeignKey(pa => pa.AllocatProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}