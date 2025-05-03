using Microsoft.AspNetCore.Http;
using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IDoctorRepository
{
    Task<List<Doctor>> GetAsync(string? name, string? speciality, List<string>? symptoms);
    Task<Doctor?> GetByEmailAsync(string email);
    Task<List<Doctor>> GetNearestDoctors(string? patientCity);
    
    Task<Doctor?> GetByIdAsync(string id);
    
    Task UpdatePassword(string id, string oldPassword,string newPassword);

    Task<string?> GetDoctorIdByUserId(string id);
    Task<Doctor?> GetByUserIdAsync(string uid);
    Task UpdateProfile(string id, string? dtoCity, string? dtoAddress, string? dtoProfileInfo,
        IFormFile? dtoProfilePicture, string? dtoSpeciality, IEnumerable<DoctorAvailability>? availability,
        string? dtoPhone, string? dtoClinicId);
    Task<List<Speciality>> GetSpecialities();
    
    Task<bool> AnyWithPmdcIdAsync(string pmdcId);
    Task<int> GetTotalDoctorsAsync();
    Task<List<Doctor>> GetForAdminAsync(string? orderBy, string? orderDirection);
    Task ToggleActiveStatus(string doctorId);
}