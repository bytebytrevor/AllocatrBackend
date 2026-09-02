using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class AllocatrUserConfiguration :
    IEntityTypeConfiguration<AllocatrUser>
{
    public void Configure(
        EntityTypeBuilder<AllocatrUser> builder
    )
    {
        builder.Property(user => user.FullName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(user => user.Location)
            .HasMaxLength(150);

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