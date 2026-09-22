using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllocatrApi.Data.Configurations;

public class CalendarFocusTaskConfiguration
    : IEntityTypeConfiguration<CalendarFocusTask>
{
    public void Configure(
        EntityTypeBuilder<CalendarFocusTask> builder)
    {
        builder.HasKey(focus => focus.Id);

        builder.HasOne(focus => focus.User)
            .WithMany()
            .HasForeignKey(focus => focus.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(focus => focus.Task)
            .WithMany()
            .HasForeignKey(focus => focus.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(focus => new
        {
            focus.UserId,
            focus.TaskId,
            focus.WeekStart
        })
        .IsUnique();
    }
}