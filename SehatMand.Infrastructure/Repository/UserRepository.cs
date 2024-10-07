using Microsoft.EntityFrameworkCore;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class UserRepository(SmDbContext dbContext) : IUserRepository
{
    public async Task SaveOtp(string? userId, string otp, DateTime expiry)
    {
        var user = await dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found for otp");
        user.Otp = otp;
        user.OtpExpiry = expiry;
        await dbContext.SaveChangesAsync();
    }

    public async Task<Otp> GetOtpAsync(string? userId)
    {
        var user = await dbContext.User.Select(u => new
        {
            u.Id,
            u.Otp,
            u.OtpExpiry
        }).FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            throw new Exception("User Not found to fetch otp by");
        }
        return new Otp(user.Otp, user.OtpExpiry);
    }

    public async Task ClearOtp(string? userId)
    {
        var user = await dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found to clear otp");
        user.Otp = null;
        user.OtpExpiry = null;
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByIdAsync(string userId)
    {
        return await dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task Activate(string? userId)
    {
        var user = await dbContext.User.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) throw new Exception("User not found to activate");
        user.IsActive = true;
        await dbContext.SaveChangesAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        
        return await dbContext.User.FirstOrDefaultAsync(u => u.Email == email);
    }
}