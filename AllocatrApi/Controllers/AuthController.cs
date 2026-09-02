using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using AllocatrApi.Dtos.Auth;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AllocatrApi.Dtos;

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
            UserName = dto.Email,
            IsAllocat = dto.IsAllocat
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
            avatarUrl = user?.AvatarUrl,
            isAllocat = user?.IsAllocat
        });
    }

    // ----------------- EMAIL VERIFICATION -----------------

    [HttpPost("confirm-email")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(
        [FromBody] ConfirmEmailDto request
    )
    {
        if (request.UserId == Guid.Empty)
        {
            return BadRequest(
                new { message = "The verification link is invalid." }
            );
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(
                new { message = "The verification link is invalid." }
            );
        }

        var user = await _userManager.FindByIdAsync(
            request.UserId.ToString()
        );

        if (user == null)
        {
            return BadRequest(
                new { message = "The verification link is invalid." }
            );
        }

        /*
        * Confirmation is intentionally idempotent.
        *
        * Opening an already-used verification link should not
        * look like an error to the user.
        */
        if (user.EmailConfirmed)
        {
            return Ok(
                new
                {
                    message = "Your email address is already verified.",
                    alreadyVerified = true,
                }
            );
        }

        string token;

        try
        {
            var tokenBytes = WebEncoders.Base64UrlDecode(
                request.Token
            );

            token = Encoding.UTF8.GetString(tokenBytes);
        }
        catch
        {
            return BadRequest(
                new
                {
                    message = "The verification link is invalid or malformed."
                }
            );
        }

        var result = await _userManager.ConfirmEmailAsync(
            user,
            token
        );

        if (!result.Succeeded)
        {
            return BadRequest(
                new
                {
                    message = "The verification link is invalid or has expired."
                }
            );
        }

        return Ok(
            new
            {
                message = "Your email address has been verified.",
                alreadyVerified = false,
            }
        );
    }
}


