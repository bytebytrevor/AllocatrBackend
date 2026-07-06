using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

class AllocatProfileSkillConfiguration : IEntityTypeConfiguration<AllocatProfileSkill>
{
    public void Configure(EntityTypeBuilder<AllocatProfileSkill> builder)
    {
        builder.HasKey(x => new { x.AllocatProfileId, x.SkillId });

        builder.HasOne(x => x.AllocatProfile)
            .WithMany(p => p.Skills)
            .HasForeignKey(x => x.AllocatProfileId);

        builder.HasOne(x => x.Skill)
            .WithMany(s => s.AllocatProfiles)
            .HasForeignKey(x => x.SkillId);
    }
}