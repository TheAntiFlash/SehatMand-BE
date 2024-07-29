using Microsoft.EntityFrameworkCore;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class DoctorRepository(SmDbContext context): IDoctorRepository
{
    public async Task<Doctor?> GetByEmailAsync(string email)
    {
        return await context.Doctor.Include(d => d.User).FirstOrDefaultAsync(x => x.Email == email);
    }
}