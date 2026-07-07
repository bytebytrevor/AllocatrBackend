namespace AllocatrApi.Models;

public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid SkillCategoryId { get; set; }
    public SkillCategory SkillCategory { get; set; } = null!;

    public ICollection<AllocatProfileSkill> AllocatProfiles { get; set; } = [];
}
