using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class CalendarPlanningBlockConfiguration
    : IEntityTypeConfiguration<CalendarPlanningBlock>
{
    public void Configure(
        EntityTypeBuilder<CalendarPlanningBlock> builder)
    {
        builder.HasKey(block => block.Id);

        builder.Property(block => block.Title)
            .HasMaxLength(180)
            .IsRequired();

        builder.Property(block => block.Notes)
            .HasMaxLength(1200);

        builder.HasOne(block => block.User)
            .WithMany()
            .HasForeignKey(block => block.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(block => block.Project)
            .WithMany()
            .HasForeignKey(block => block.ProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(block => block.Task)
            .WithMany()
            .HasForeignKey(block => block.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(block => new
        {
            block.UserId,
            block.StartAt
        });
    }
}