using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IUserRepository
{
    Task SaveOtp(string? userId, string otp, DateTime expiryDateTime);
    Task<Otp> GetOtpAsync(string? userId);
    Task ClearOtp(string? userId);
    Task<User?> GetByIdAsync(string userId);

    Task Activate(string? userId);
    Task<User?> GetByEmailAsync(string email);
}