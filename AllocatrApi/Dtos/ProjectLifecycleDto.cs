using System.ComponentModel.DataAnnotations;

namespace AllocatrApi.Dtos;

public sealed class ProjectLifecycleRequest
{
    [Required, StringLength(1000)]
    public string Reason { get; set; } = string.Empty;
}

public sealed record ProjectLifecycleResult(Guid ProjectId, Guid EventId, bool Changed);
