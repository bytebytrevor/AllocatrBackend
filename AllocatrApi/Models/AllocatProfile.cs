using System;

namespace AllocatrApi.Models;

public class AllocatProfile
{
    // public Guid Id { get; set; }

    public string AllocatrUserId { get; set; } = string.Empty;
    public AllocatrUser AllocatrUser { get; set; } = null!;

    public string? IdNumber { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? Bio { get; set; }
    public string? Availability { get; set; }
    public int? YearsExperience { get; set; }
    public bool IsVisible { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

}