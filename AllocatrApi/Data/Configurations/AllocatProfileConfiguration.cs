using System;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class AllocatProfileConfiguration : IEntityTypeConfiguration<AllocatProfile>
{
    public void Configure(EntityTypeBuilder<AllocatProfile> entity)
    {
        entity.ToTable("AllocatProfiles");

        entity.HasKey(a => a.AllocatrUserId);

        entity.HasOne(a => a.AllocatrUser)
            .WithOne(u => u.AllocatProfile)
            .HasForeignKey<AllocatProfile> (a => a.AllocatrUserId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.Property(a => a.AllocatrUserId)
            .IsRequired();
    }
}