namespace AllocatrApi.Services;

public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null
    );

    Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl
    );
}