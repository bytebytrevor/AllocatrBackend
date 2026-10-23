using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class AllocatrUserConfiguration : IEntityTypeConfiguration<AllocatrUser>
{
    public void Configure(EntityTypeBuilder<AllocatrUser> builder)
    {
        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(user => user.Location)
            .HasMaxLength(150);

        builder.Property(user => user.CountryCode)
            .HasMaxLength(2)
            .IsUnicode(false);

        builder.HasOne(user => user.Country)
            .WithMany()
            .HasForeignKey(user => user.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(user => user.CityName)
            .HasMaxLength(150);

        builder.HasIndex(user => new { user.CountryCode, user.CityName });

        builder.Property(user => user.AvatarUrl)
            .HasMaxLength(1000);

        builder.Property(user => user.IsAllocat)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(user => user.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .ValueGeneratedOnAdd();

        builder.HasIndex(user => user.IsAllocat);
    }
}
