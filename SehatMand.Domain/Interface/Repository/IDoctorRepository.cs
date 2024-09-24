using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IDoctorRepository
{
    Task<Doctor?> GetByEmailAsync(string email);
    Task<List<Doctor>> GetNearestDoctors(string? patientCity);
    
    Task<Doctor> getByIdAsync(string id);
    
}