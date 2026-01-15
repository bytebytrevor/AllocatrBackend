using System;

namespace AllocatrApi.Models;

public class ProjectTag
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Tag { get; set; } = null!;
}
