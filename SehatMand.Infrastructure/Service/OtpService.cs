using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class OtpService(IEmailService emailService, IUserRepository userRepo): IOtpService
{
    public static string GenerateOtp(int length = 4)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(1, length).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
    }

    public async Task SendOtp(string otp, string email, string firstName, int expireInMinutes)
    {
        await emailService.SendOtpEmailAsync(email, otp, firstName, expireInMinutes);
    }
    
    public async Task SaveOtpForUserAsync(string? userId, string otp, int expireInMinutes)
    {
        await userRepo.SaveOtp(userId, otp, DateTime.Now.AddMinutes(expireInMinutes));
    }

    public async Task<bool> VerifyOtpAsync(string? userId, string otp)
    {
        var dbOtp = await userRepo.GetOtpAsync(userId);

        if (dbOtp.Value is null || dbOtp.Expiry is null)
        {
            throw new Exception("First Generate an Otp!");
        }

        if (dbOtp.Value != otp || dbOtp.Expiry < DateTime.Now)
        {
            return false;
        }

        await userRepo.ClearOtp(userId);
        return true;

    }
}