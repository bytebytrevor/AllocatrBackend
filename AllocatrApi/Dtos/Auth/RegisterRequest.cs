using System;
using System.ComponentModel.DataAnnotations;

namespace AllocatrApi.Dtos.Auth;

public class RegisterRequest
{
    [MinLength(2)]
    public required string FullName { get; init; }

    [EmailAddress]
    public required string Email { get; init; }

    [MinLength(8)]
    public required string Password { get; init; }
}
