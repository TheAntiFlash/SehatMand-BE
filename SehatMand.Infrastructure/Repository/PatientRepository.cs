using Microsoft.EntityFrameworkCore;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Infrastructure.Repository;

public class PatientRepository(SmDbContext context): IPatientRepository
{
    public async Task<Patient?> GetByEmailAsync(string email)
    {
        return await context.Patient.Include(d => d.User).FirstOrDefaultAsync(x => x.Email == email);
    }
    
    public async Task<bool> CompletePatientProfile(
        string id,
        string address,
        string city,
        float height,
        float weight,
        string gender,
        string bloodGroup,
        DateTime dateOfBirth
    )
    {
        var patient = await context.Patient.FirstOrDefaultAsync(p => p.UserId == id);
        if (patient == null) throw new Exception("Patient Not Found");
        patient.Address = address;
        patient.Height = height;
        patient.Weight = weight;
        patient.BloodGroup = bloodGroup;
        patient.City = city;
        patient.Gender = gender;
        patient.DateOfBirth = dateOfBirth;
        
        return await context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdatePatientProfile(
        string? id,
        string? fullName,
        string? email,
        string? phoneNumber,
        string? address,
        string? city,
        float height,
        float weight,
        string? gender,
        string? bloodGroup,
        DateTime? dateOfBirth,
        string? profileInfo
    )
    {
        var patient = await context.Patient.FirstOrDefaultAsync(p => p.UserId == id);
        if (patient == null) throw new Exception("Patient Not Found");
        if (fullName != null) patient.Name = fullName;
        if (email != null) patient.Email = email;
        if (phoneNumber != null) patient.Phone = phoneNumber;
        if (address != null) patient.Address = address;
        if (height != 0.0) patient.Height = height;
        if (weight != 0.0) patient.Weight = weight;
        if (!string.IsNullOrWhiteSpace(bloodGroup)) patient.BloodGroup = bloodGroup;
        if (city != null) patient.City = city;
        if (!string.IsNullOrWhiteSpace(gender)) patient.Gender = gender;
        if (dateOfBirth.HasValue) patient.DateOfBirth = dateOfBirth.Value;
        if (profileInfo != null) patient.ProfileInfo = profileInfo;
        
        return await context.SaveChangesAsync() > 0;
    }

    public async Task UpdatePatientPassword(string id, string oldPassword, string newPassword)
    {
        var patient = await GetByIdAsync(id);
        Console.WriteLine(patient);
        if (patient == null) throw new Exception("Patient not found");
        patient.User = await context.User.FirstOrDefaultAsync(u => u.Id == patient.UserId);
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, patient.User.PasswordHash))
        {
            throw new Exception("Invalid Password");
        }
        patient.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        
        await context.SaveChangesAsync();
    }

    public async Task<Patient?> GetByIdAsync(string id)
    {
        return await context.Patient.FirstOrDefaultAsync(p => p.UserId == id);
    }

    public async Task<string?> GetPatientIdByUserId(string userId)
    {
        var patient = await context.Patient.Select(u => new {u.Id, u.UserId}).FirstOrDefaultAsync(p => p.UserId == userId);
        return patient?.Id;
    }
}