using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectAllocatConfiguration : IEntityTypeConfiguration<ProjectAllocat>
{
    public void Configure(EntityTypeBuilder<ProjectAllocat> builder)
    {
        builder.HasKey(pa => new { pa.ProjectId, pa.AllocatId }); // composite key
        builder.HasOne(pa => pa.Project)
               .WithMany(p => p.AllocatAssignments)
               .HasForeignKey(pa => pa.ProjectId);

        builder.HasOne(pa => pa.Allocat)
               .WithMany()
               .HasForeignKey(pa => pa.AllocatId);
    }
}
