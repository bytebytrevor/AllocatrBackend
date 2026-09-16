namespace AllocatrApi.Models;

public class ProjectSkill
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}