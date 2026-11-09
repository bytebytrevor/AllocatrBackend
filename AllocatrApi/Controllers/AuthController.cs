using System.Text;
using AllocatrApi.Dtos;
using AllocatrApi.Middleware;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AllocatrUser> _users;
    private readonly SignInManager<AllocatrUser> _signIn;
    private readonly EmailVerificationSender _verification;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<AllocatrUser> users,
        SignInManager<AllocatrUser> signIn, EmailVerificationSender verification,
        ILogger<AuthController> logger)
    {
        _users = users;
        _signIn = signIn;
        _verification = verification;
        _logger = logger;
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth-attempts")]
    [HttpPost("register")]
    public async Task<IActionResult> Register(AllocatrApi.Dtos.Auth.RegisterRequest dto)
    {
        var email = dto.Email.Trim();
        if (await _users.FindByEmailAsync(email) != null)
            return Conflict(new { message = "User already exists" });
        var user = new AllocatrUser
        {
            FullName = dto.FullName.Trim(), Email = email,
            UserName = email, IsAllocat = dto.IsAllocat,
        };
        var created = await _users.CreateAsync(user, dto.Password);
        if (!created.Succeeded) return BadRequest(created.Errors);

        // Account creation and delivery are separate outcomes. Never make the
        // user register a second time after an SMTP failure.
        var verificationEmailSent = false;
        try
        {
            await _verification.SendAsync(user);
            verificationEmailSent = true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Initial verification delivery failed for user {UserId}", user.Id);
        }
        return Ok(new
        {
            message = "Registered successfully", verificationEmailSent,
            emailVerificationRequired = true,
        });
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth-attempts")]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest dto)
    {
        var user = await _users.FindByEmailAsync(dto.Email.Trim());
        if (user == null) return Unauthorized(new { message = "Invalid credentials" });
        // Unverified users may obtain a session for /me, resend and logout.
        // RequireVerifiedEmailMiddleware protects the rest of the API.
        var result = await _signIn.PasswordSignInAsync(user, dto.Password, true, true);
        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials" });
        return Ok(new { email = user.Email, fullName = user.FullName });
    }

    [Authorize, AllowUnverifiedEmail]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return NoContent();
    }

    [Authorize, AllowUnverifiedEmail]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        // Profile existence indicates that the first-time create step was saved.
        // This is deliberately distinct from the 100% account-completion badge.
        var hasAllocatProfile = user.IsAllocat && await _users.Users
            .Where(item => item.Id == user.Id)
            .Select(item => item.AllocatProfile != null)
            .SingleAsync();
        return Ok(new
        {
            userId = user.Id, email = user.Email, fullName = user.FullName,
            avatarUrl = user.AvatarUrl, isAllocat = user.IsAllocat,
            emailConfirmed = user.EmailConfirmed, hasAllocatProfile,
        });
    }

    [Authorize, AllowUnverifiedEmail]
    [EnableRateLimiting("verification-email")]
    [HttpPost("email-verification")]
    public async Task<IActionResult> ResendVerification()
    {
        var user = await _users.GetUserAsync(User);
        if (user == null) return Unauthorized();
        if (user.EmailConfirmed)
            return Ok(new { alreadyVerified = true, message = "Your email is already verified." });
        try
        {
            await _verification.SendAsync(user);
            return Ok(new { alreadyVerified = false, message = "Verification email sent.", retryAfterSeconds = 60 });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Verification delivery failed for user {UserId}", user.Id);
            return StatusCode(503, new { message = "Your account is saved, but the verification email could not be sent. Please try again shortly." });
        }
    }

    [AllowAnonymous]
    [EnableRateLimiting("auth-attempts")]
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto request)
    {
        if (request.UserId == Guid.Empty || string.IsNullOrWhiteSpace(request.Token))
            return BadRequest(new { message = "The verification link is invalid." });
        var user = await _users.FindByIdAsync(request.UserId.ToString());
        if (user == null)
            return BadRequest(new { message = "The verification link is invalid." });
        if (user.EmailConfirmed)
            return Ok(new { message = "Your email address is already verified.", alreadyVerified = true });
        string token;
        try { token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token)); }
        catch (FormatException)
        {
            return BadRequest(new { message = "The verification link is invalid or malformed." });
        }
        var result = await _users.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return BadRequest(new { message = "The verification link is invalid or has expired." });
        return Ok(new { message = "Your email address has been verified.", alreadyVerified = false });
    }
}
