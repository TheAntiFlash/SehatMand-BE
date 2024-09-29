namespace SehatMand.Domain.Interface.Service;

public interface IEmailService
{
    public Task SendOtpEmailAsync(string email, string otp, string firstName, int expireInMinutes);
}