namespace SehatMand.Domain.Interface.Service;

public interface IOtpService
{
    public Task SendOtp(string otp, string email, string firstName, int expireInMinutes);
    public Task SaveOtpForUserAsync(string? userId, string otp, int expireInMinutes);
    public Task<bool> VerifyOtpAsync(string? userId, string otp);
    
    
}