using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Models;

public class AllocatrUser : IdentityUser
{
    public required string FullName { get; set; } = null!;
    public string? IdNumber { get; set; }
    public bool IsAllocat { get; set; } = false;

    public string? ProfileImageUrl { get; set; }
}
