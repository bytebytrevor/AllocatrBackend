using System;
using Microsoft.AspNetCore.Identity;

namespace AllocatrApi.Models;

public class AllocatrUser : IdentityUser
{
    // Extend here
    public string Fullname {get; set; } ="";
    public bool IsAllocat { get; set; } = false;
}
