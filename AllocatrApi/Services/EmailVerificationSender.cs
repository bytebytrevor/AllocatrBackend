using System.Text;
using AllocatrApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace AllocatrApi.Services;

// Register as scoped. The URL comes from configuration, never a request header.
public sealed class EmailVerificationSender
{
    private readonly UserManager<AllocatrUser> _users;
    private readonly IEmailService _email;
    private readonly IConfiguration _configuration;

    public EmailVerificationSender(UserManager<AllocatrUser> users,
        IEmailService email, IConfiguration configuration)
    {
        _users = users;
        _email = email;
        _configuration = configuration;
    }

    public async Task SendAsync(AllocatrUser user)
    {
        if (user.EmailConfirmed) return;
        if (string.IsNullOrWhiteSpace(user.Email))
            throw new InvalidOperationException("The account has no email address.");

        var configured = _configuration["EmailVerification:FrontendBaseUrl"];
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var origin) ||
            !string.IsNullOrEmpty(origin.Query) || !string.IsNullOrEmpty(origin.Fragment) ||
            !string.IsNullOrEmpty(origin.UserInfo) ||
            (origin.Scheme != Uri.UriSchemeHttps &&
             !(origin.Scheme == Uri.UriSchemeHttp && origin.IsLoopback)))
            throw new InvalidOperationException("Configure EmailVerification:FrontendBaseUrl with your frontend HTTPS URL (HTTP is allowed on localhost).");

        var token = await _users.GenerateEmailConfirmationTokenAsync(user);
        var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        var url = QueryHelpers.AddQueryString(
            origin.AbsoluteUri.TrimEnd('/') + "/verify-email",
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id.ToString(),
                ["token"] = encoded,
            });
        await _email.SendEmailVerificationAsync(user.Email, user.FullName, url);
    }
}
