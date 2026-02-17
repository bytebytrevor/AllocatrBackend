using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Models;

public class AllocatrUser : IdentityUser
{
    public required string FullName { get; set; } = null!;
    public string? IdNumber { get; set; }
    public bool IsAllocat { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public ICollection<TaskComment>? TaskComments { get; set; } = new List<TaskComment>();
    public ICollection<TaskItem>? AssignedTasks { get; set; }
        = new List<TaskItem>();

    public ICollection<TaskItem>? CreatedTasks { get; set; }
        = new List<TaskItem>();
}
