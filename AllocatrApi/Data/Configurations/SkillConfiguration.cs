using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(s => s.Name)
            .IsUnique();

    }
}