using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using AllocatrApi.Models;
using AllocatrApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PasswordResetController(UserManager<AllocatrUser> users,
    SignInManager<AllocatrUser> signIn, PasswordResetSender sender,
    ILogger<PasswordResetController> logger) : ControllerBase
{
    [HttpPost("forgot-password")]
    [EnableRateLimiting("password-reset-request")]
    [RequestSizeLimit(8192)]
    public async Task<IActionResult> Forgot([FromBody] ForgotPasswordDto request)
    {
        var user = await users.FindByEmailAsync(request.Email.Trim());
        if (user != null && await users.HasPasswordAsync(user))
        {
            try { await sender.SendAsync(user); }
            catch (Exception exception)
            {
                // Neither delivery failure nor account existence changes the response.
                logger.LogError(exception, "Password reset email could not be sent for user {UserId}", user.Id);
            }
        }
        return Ok(new { message = "If an account uses that email address, a password reset link will be sent.", retryAfterSeconds = 60 });
    }

    [HttpPost("reset-password")]
    [EnableRateLimiting("auth-attempts")]
    [RequestSizeLimit(16384)]
    public async Task<IActionResult> Reset([FromBody] ResetPasswordDto request)
    {
        if (request.UserId == Guid.Empty) return InvalidLink();
        var user = await users.FindByIdAsync(request.UserId.ToString());
        if (user is null) return InvalidLink();
        string token;
        try { token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token)); }
        catch (FormatException) { return InvalidLink(); }
        catch (ArgumentException) { return InvalidLink(); }
        var result = await users.ResetPasswordAsync(user, token, request.Password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(error => error.Code == "InvalidToken")) return InvalidLink();
            return BadRequest(new { code = "password_requirements", message = "The password does not meet the account requirements. Use at least 8 characters, including uppercase and lowercase letters and a number." });
        }
        // Identity updates the password hash and security stamp. Do not confirm
        // email, change account type, or automatically sign the recipient in.
        if (User.FindFirstValue(ClaimTypes.NameIdentifier) == user.Id.ToString())
            await signIn.SignOutAsync();
        return Ok(new { message = "Your password has been reset. Sign in with your new password." });
    }
    private BadRequestObjectResult InvalidLink() => BadRequest(new
    {
        code = "invalid_reset_link",
        message = "This password reset link is invalid or has expired. Request a new one.",
    });
}

public sealed class ForgotPasswordDto
{
    [Required, EmailAddress, StringLength(254)]
    public string Email { get; set; } = "";
}
public sealed class ResetPasswordDto
{
    public Guid UserId { get; set; }
    [Required, StringLength(4096)] public string Token { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 8)] public string Password { get; set; } = "";
}
