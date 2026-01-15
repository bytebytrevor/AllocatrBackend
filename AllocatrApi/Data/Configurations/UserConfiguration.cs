using System;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace AllocatrApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<AllocatrUser>
{
    public void Configure(EntityTypeBuilder<AllocatrUser> entity)
    {
        entity.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(60);

        entity.Property(u => u.IsAllocat)
            .IsRequired();

        entity.HasIndex(u => u.IsAllocat);
    }
}
