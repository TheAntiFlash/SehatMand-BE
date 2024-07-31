using Microsoft.EntityFrameworkCore;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class PatientRepository(SmDbContext context): IPatientRepository
{
    public async Task<Patient?> GetByEmailAsync(string email)
    {
        return await context.Patient.Include(d => d.User).FirstOrDefaultAsync(x => x.Email == email);
    }
}