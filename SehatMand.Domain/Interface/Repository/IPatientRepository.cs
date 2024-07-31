using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IPatientRepository
{
    public Task<Patient?> GetByEmailAsync(string email);
}