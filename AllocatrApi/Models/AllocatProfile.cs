namespace AllocatrApi.Models;

public class AllocatProfile
{
    public Guid AllocatrUserId { get; set; }
    public AllocatrUser AllocatrUser { get; set; } = null!;

    public string? IdNumber { get; set; }

    public string? Title { get; set; }
    public string? Headline { get; set; }
    public string? Bio { get; set; }

    public decimal? HourlyRate { get; set; }
    public string Currency { get; set; } = "USD";

    public string Availability { get; set; } = "available";
    public int? YearsExperience { get; set; }

    public decimal AverageRating { get; set; }
    public int RatingCount { get; set; }

    public bool IsVerified { get; set; }
    public int? AverageResponseTimeMinutes { get; set; }
    public int Level { get; set; } = 1;

    public bool IsVisible { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<AllocatProfileSkill> Skills { get; set; } = [];
    public ICollection<ProjectAllocat> ProjectAssignments { get; set; } = [];
}