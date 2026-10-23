using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Models;

public class AllocatrUser : IdentityUser<Guid>
{
    public required string FullName { get; set; } = null!;

    // Retained temporarily for existing account data and API compatibility.
    public string? Location { get; set; }

    // Residence: any country in the worldwide catalogue and a free-text city.
    // Nullable so existing accounts do not receive an inferred residence.
    public string? CountryCode { get; set; }
    public Country? Country { get; set; }
    public string? CityName { get; set; }

    public string? AvatarUrl { get; set; }
    public Guid? AvatarFileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAllocat { get; set; } = false;
    public AllocatProfile? AllocatProfile { get; set; }
    public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Review> ReviewsWritten { get; set; } = [];
}
