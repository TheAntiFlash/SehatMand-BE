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
    
    public async Task<bool> UpdatePatientProfile(
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
}