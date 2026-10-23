using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.ToTable("Cities");
        builder.HasKey(city => city.Id);

        builder.Property(city => city.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(city => city.CountryCode)
            .IsRequired()
            .HasMaxLength(2)
            .IsUnicode(false);

        builder.HasOne(city => city.Country)
            .WithMany(country => country.Cities)
            .HasForeignKey(city => city.CountryCode)
            .OnDelete(DeleteBehavior.Restrict);

        // Allows a user's city/country pair to reference the same city record.
        builder.HasAlternateKey(city => new { city.Id, city.CountryCode });

        builder.HasIndex(city => new { city.CountryCode, city.Name });

        // Stable negative IDs reserve positive generated IDs for future additions.
        // Keep these IDs unchanged after the migration has been applied.
        builder.HasData(
            new City { Id = -1, Name = "Harare", CountryCode = "ZW" },
            new City { Id = -2, Name = "Bulawayo", CountryCode = "ZW" },
            new City { Id = -3, Name = "Chitungwiza", CountryCode = "ZW" },
            new City { Id = -4, Name = "Mutare", CountryCode = "ZW" },
            new City { Id = -5, Name = "Gweru", CountryCode = "ZW" },
            new City { Id = -6, Name = "Masvingo", CountryCode = "ZW" },
            new City { Id = -7, Name = "Kwekwe", CountryCode = "ZW" },
            new City { Id = -8, Name = "Kadoma", CountryCode = "ZW" },
            new City { Id = -9, Name = "Victoria Falls", CountryCode = "ZW" },
            new City { Id = -10, Name = "Marondera", CountryCode = "ZW" },
            new City { Id = -11, Name = "Johannesburg", CountryCode = "ZA" },
            new City { Id = -12, Name = "Cape Town", CountryCode = "ZA" },
            new City { Id = -13, Name = "Pretoria", CountryCode = "ZA" },
            new City { Id = -14, Name = "Durban", CountryCode = "ZA" },
            new City { Id = -15, Name = "Gqeberha", CountryCode = "ZA" },
            new City { Id = -16, Name = "Bloemfontein", CountryCode = "ZA" },
            new City { Id = -17, Name = "East London", CountryCode = "ZA" },
            new City { Id = -18, Name = "Polokwane", CountryCode = "ZA" },
            new City { Id = -19, Name = "Mbombela", CountryCode = "ZA" },
            new City { Id = -20, Name = "Kimberley", CountryCode = "ZA" }
        );
    }
}
