using System;
using System.Security.Claims;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AllocatrApi.Dtos.Auth;

namespace AllocatrApi.Controllers;



[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _userManager;
    private readonly SignInManager<AllocatrUser> _signInManager;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<AllocatrUser> userManager,
        SignInManager<AllocatrUser> signInManager,
        TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }


    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new { userId, email });
    }

    /************************** REGISTER *************************/
    [HttpPost("register")]
    public async Task<IActionResult> Register(Dtos.Auth.RegisterRequest dto)
    {
        // Fast-path check
        if (await _userManager.FindByEmailAsync(dto.Email) != null)
        {
            return Conflict(new { message = "User already exists" });
        }

        var user = new AllocatrUser
        {
            FullName = dto.FullName,
            UserName = dto.Email,
            Email = dto.Email,
        };

        try
        {
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "Registered successfully" });
        }
        catch (DbUpdateException)
        {
            // Safety net for race conditions
            return Conflict(new { message = "User already exists" });
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, new { message = "Registration failed" });
        }
    }

    /************************** LOGIN *************************/
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(
            user, dto.Password, true);

        if (!result.Succeeded) return Unauthorized();

        var token = _tokenService.CreateToken(user);

        Response.Cookies.Append("access_token", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, //Swith to true for production
            // Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/"
        });

        return Ok(new { user.Email });
    }

    /************************** LOGOUT*************************/
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = false, //Swith back to true for production
            // Secure = true,
            SameSite = SameSiteMode.Lax, //Swith to None for production
            Path = "/"
        });

        return NoContent();
    }
}
