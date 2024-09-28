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
    
    public async Task<Doctor?> GetByIdAsync(string id)
    {
        return await context.Doctor
            .Include(d => d.DoctorDailyAvailability)
            .Include(d => d.Qualifications)
            .Include(d => d.Appointment)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    
    public async Task UpdatePassword(string id, string oldPassword, string newPassword)
    {
        var doctor = await GetByIdAsync(id);
        Console.WriteLine(doctor);
        if (doctor == null) throw new Exception("Doctor Not Found");
        
            
        Console.WriteLine(doctor.User);
        doctor.User = await context.User.FirstOrDefaultAsync(u => u.Id == doctor.UserId);
        
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, doctor.User.PasswordHash))
        {
            throw new Exception("Invalid Password");
        }
        
        // Check if the 'User' is null
        if (doctor.User == null) throw new Exception("User associated with doctor not found");
    
        doctor.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await context.SaveChangesAsync();
    }

    public Task<string?> GetDoctorIdByUserId(string id)
    {
        return context.Doctor.Where(d => d.UserId == id).Select(d => d.Id).FirstOrDefaultAsync();
    }

    public async Task<Doctor?> GetByUserIdAsync(string uid)
    {
        return await context.Doctor
            .Include(d => d.DoctorDailyAvailability)
            .Include(d => d.Qualifications)
            .Include(d => d.Appointment)
            .FirstOrDefaultAsync(d => d.UserId == uid);
    }
}