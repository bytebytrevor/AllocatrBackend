using System;

namespace AllocatrApi.Models;

public class AllocatProfileSkill
{
    public Guid AllocatProfileId { get; set; }
    public AllocatProfile AllocatProfile { get; set; } = null!;

    public Guid SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
}
