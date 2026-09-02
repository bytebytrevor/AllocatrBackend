// using Microsoft.AspNetCore.Identity;

// namespace AllocatrApi.Models;

// public class AllocatrUser : IdentityUser<Guid>
// {
//     public required string FullName { get; set; } = null!;
//     public bool IsAllocat { get; set; } = false;
//     public string? AvatarUrl { get; set; }
//     public AllocatProfile? AllocatProfile { get; set; }
//     public ICollection<TaskComment>? TaskComments { get; set; } = new List<TaskComment>();
//     public ICollection<TaskItem>? AssignedTasks { get; set; } = new List<TaskItem>();
//     public ICollection<TaskItem>? CreatedTasks { get; set; } = new List<TaskItem>();
//     public ICollection<Review> ReviewsWritten { get; set; } = [];
// }

using Microsoft.AspNetCore.Identity;
namespace AllocatrApi.Models;
public class AllocatrUser : IdentityUser<Guid>
{
    public required string FullName { get; set; } = null!;
    public string? Location { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAllocat { get; set; } = false;
    public AllocatProfile? AllocatProfile { get; set; }
    public ICollection<TaskComment> TaskComments { get; set; } = new List<TaskComment>();
    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public ICollection<Review> ReviewsWritten { get; set; } = [];
}