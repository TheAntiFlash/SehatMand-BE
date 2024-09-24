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

    public async Task<List<Doctor>> GetNearestDoctors(string? patientCity)
    {
        return await context.Doctor.Include(d => d.Qualifications).Where(d => d.City != null && d.City.Equals(patientCity, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();
    }
    
    public async Task<Doctor> getByIdAsync(string id)
    {
        return await context.Doctor.FirstOrDefaultAsync(d => d.UserId == id);
    }
}