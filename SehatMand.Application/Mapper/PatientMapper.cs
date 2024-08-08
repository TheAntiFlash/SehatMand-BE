using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Patient;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class PatientMapper
{
    public static Patient ToPatient(this RegisterPatientDto dto)
    {
        var patient = new Patient
        {
            Name = dto.FullName,
            Email = dto.Email,
            Phone = dto.PhoneNumber,
            User = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            }
        };
        patient.UserId = patient.User.Id;
        
        patient.User.Role = "Patient";
        patient.CreatedAt = DateTime.Now;
        patient.User.IsActive = true;
        return patient;
    }
    
    

}