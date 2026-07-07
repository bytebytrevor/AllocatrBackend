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

    public async Task<SkillCategoryDto> CreateSkillCategoryAsync(CreateSkillCategoryDto dto)
    {
        var skillCategory = new SkillCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.SkillCategories.Add(skillCategory);
        await _db.SaveChangesAsync();

        return new SkillCategoryDto(
            skillCategory.Id,
            skillCategory.Name
        );
    }

    public async Task<SkillCategoryDto?> GetSkillCategoryByIdAsync(Guid id)
    {
        return await _db.SkillCategories
            .Where(sc => sc.Id == id)
            .Select(sc => new SkillCategoryDto(
                sc.Id,
                sc.Name
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<List<SkillCategoryDto>> GetAllSkillCategoriesAsync()
    {
        return await _db.SkillCategories
            .OrderBy(sc => sc.Name)
            .Select(sc => new SkillCategoryDto(
                sc.Id,
                sc.Name
            )).ToListAsync();
    }
}