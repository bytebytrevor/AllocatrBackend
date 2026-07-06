using System.ComponentModel.DataAnnotations;
using AllocatrApi.Data;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Services;

public class SkillCategoryService
{
    private readonly AllocatrDbContext _db;

    public SkillCategoryService(AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<SkillCategory> CreateSkillCategoryAsync(SkillCategory skillCategory)
    {
        skillCategory.CreatedAt = DateTime.Now;
        skillCategory.UpdatedAt = DateTime.Now;

        _db.SkillCategories.Add(skillCategory);
        await _db.SaveChangesAsync();

        return skillCategory;
    }

    public async Task<List<SkillCategoryDto>> GetAllSkillCategories()
    {
        return await _db.SkillCategories
            .Select(sc => new SkillCategoryDto(
                sc.Id,
                sc.Name
            )).ToListAsync();
    }
}