using System;
using AllocatrApi.Configurations;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Data;

public class AllocatrDbContext(DbContextOptions<AllocatrDbContext> options)
    : IdentityDbContext<AllocatrUser>(options)
{
    public DbSet<Project> Projects { get; set; } = null!;
    public DbSet<TaskItem> TaskItems { get; set; } = null!;
    public DbSet<ProjectMessage> ProjectMessages { get; set; } = null!;
    public DbSet<ProjectTag> ProjectTags { get; set; } = null!;
    public DbSet<ProjectAllocat> ProjectAllocats { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AllocatrDbContext).Assembly
        );
    }
}
