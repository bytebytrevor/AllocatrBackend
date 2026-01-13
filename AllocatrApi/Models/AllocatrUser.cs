using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Models;

public class AllocatrUser : IdentityUser
{
    // Extend here
    [Required]
    public required string FullName { get; set; } = null!;
    public string? IdNumber { get; set; }
    [Required]
    public bool IsAllocat { get; set; } = false;
}
