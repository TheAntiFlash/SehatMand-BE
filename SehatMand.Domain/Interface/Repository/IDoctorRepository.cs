using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IDoctorRepository
{
    Task<Doctor?> GetByEmailAsync(string email);
}