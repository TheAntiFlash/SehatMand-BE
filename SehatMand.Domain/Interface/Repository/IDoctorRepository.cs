using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IDoctorRepository
{
    Task<Doctor?> GetByEmailAsync(string email);
    Task<List<Doctor>> GetNearestDoctors(string? patientCity);
    
    Task<Doctor?> GetByIdAsync(string id);
    
    Task UpdatePassword(string id, string oldPassword,string newPassword);

    Task<string?> GetDoctorIdByUserId(string id);
}