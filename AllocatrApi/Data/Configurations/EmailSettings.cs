namespace AllocatrApi.Configuration;

public class EmailSettings
{
    public const string SectionName = "Email";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 465;

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Allocatr";

    public bool CheckCertificateRevocation { get; set; } = true;
}