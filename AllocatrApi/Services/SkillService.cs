using AllocatrApi.Data;
using AllocatrApi.Models;
using Microsoft.EntityFrameworkCore;
namespace AllocatrApi.Services;

public class SkillService
{
    private readonly AllocatrDbContext _db;

    public SkillService(AllocatrDbContext db)
    {
        _db = db;
    }

    public async Task<SkillDto> CreateSkillAsync(CreateSkillDto dto)
    {
        var category = await _db.SkillCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == dto.SkillCategoryId) ?? throw new KeyNotFoundException(
                "The selected skill category does not exist."
            );

        var normalizedName = dto.Name.Trim();

        var alreadyExists = await _db.Skills.AnyAsync(s =>
            s.SkillCategoryId == dto.SkillCategoryId &&
            s.Name.ToLower().Equals(normalizedName.ToLower())
        );

        if (alreadyExists)
            throw new InvalidOperationException(
                "A skill with that name already exists in this category."
            );

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            SkillCategoryId = dto.SkillCategoryId
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync();

        return new SkillDto(
            skill.Id,
            skill.Name,
            category.Id,
            category.Name
        );
    }

    public async Task<SkillDto?> GetSkillByIdAsync(Guid id)
    {
        return await _db.Skills
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SkillDto(
                s.Id,
                s.Name,
                s.SkillCategoryId,
                s.SkillCategory.Name
            ))
            .FirstOrDefaultAsync();
    }
}