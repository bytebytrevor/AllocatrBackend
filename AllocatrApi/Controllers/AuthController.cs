using AllocatrApi.Dtos.Auth;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly SignInManager<AllocatrUser> _signInManager;

    public AuthController(UserManager<AllocatrUser> userManager, SignInManager<AllocatrUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    // ----------------- REGISTER -----------------
    [HttpPost("register")]
    public async Task<IActionResult> Register(Dtos.Auth.RegisterRequest dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return Conflict(new { message = "User already exists" });

        var user = new AllocatrUser
        {
            FullName = dto.FullName,
            Email = dto.Email,
            UserName = dto.Email
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Registered successfully" });
    }

    // ----------------- LOGIN -----------------
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });

        var result = await _signInManager.PasswordSignInAsync(user, dto.Password, true, false);
        if (!result.Succeeded) return Unauthorized(new { message = "Invalid credentials" });

        // Identity sets the cookie automatically
        return Ok(new { email = user.Email, fullName = user.FullName });
    }

    // ----------------- LOGOUT -----------------
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    // ----------------- CURRENT USER -----------------
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            return Unauthorized();

        return Ok(new
        {
            userId = user.Id,
            email = user.Email,
            fullName = user.FullName,
            // avatarUrl = user.AvatarUrl
        });
    }



}


