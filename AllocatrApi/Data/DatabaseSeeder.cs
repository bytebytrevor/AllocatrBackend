using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AllocatrDbContext db)
    {
        await SeedSkillCategoriesAsync(db);
    }

    private static async Task SeedSkillCategoriesAsync(AllocatrDbContext db)
    {
        if (await db.SkillCategories.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var categories = new List<SkillCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Home Services", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Automotive", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Construction", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Cleaning & Laundry", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Transport & Logistics", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Personal Services", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Childcare & School Runs", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Repairs & Maintenance", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Events & Entertainment", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Education & Training", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Health & Wellness", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Beauty & Grooming", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Agriculture & Gardening", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Hospitality & Food", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Security Services", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Business Support", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Professional Services", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Technology", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Creative & Media", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Other Services", CreatedAt = now, UpdatedAt = now },
            new() { Id = Guid.NewGuid(), Name = "Pet & Animal Care", CreatedAt = now, UpdatedAt = now },
        };

        db.SkillCategories.AddRange(categories);
        await db.SaveChangesAsync();
    }
}