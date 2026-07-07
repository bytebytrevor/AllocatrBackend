using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Data.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(s => new { s.SkillCategoryId, s.Name })
            .IsUnique();

        builder.HasOne(s => s.SkillCategory)
            .WithMany(sc => sc.Skills)
            .HasForeignKey(s => s.SkillCategoryId);

    }
}