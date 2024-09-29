namespace SehatMand.Domain.Utils.Smtp;

public record SmtpSettings
{
    public string Host { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}