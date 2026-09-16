using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class ProjectSkillConfiguration
    : IEntityTypeConfiguration<ProjectSkill>
{
    public void Configure(
        EntityTypeBuilder<ProjectSkill> builder
    )
    {
        builder.HasKey(ps => new
        {
            ps.ProjectId,
            ps.SkillId
        });

        builder
            .HasOne(ps => ps.Project)
            .WithMany(p => p.ProjectSkills)
            .HasForeignKey(ps => ps.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ps => ps.Skill)
            .WithMany(s => s.ProjectSkills)
            .HasForeignKey(ps => ps.SkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}