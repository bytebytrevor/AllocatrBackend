namespace AllocatrApi.Models;

public class Skill
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public SkillCategory Category { get; set; } = null!;

    public ICollection<AllocatProfileSkill> AllocatProfiles { get; set; } = [];
}
