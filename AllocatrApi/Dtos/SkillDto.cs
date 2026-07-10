public record class SkillDto(
    Guid Id,
    string Name,
    Guid SkillCategoryId,
    string SkillCategory
);