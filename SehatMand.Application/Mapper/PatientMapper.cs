using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Patient;
using SehatMand.Application.Dto.Utils;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class PatientMapper
{
    public static Patient? ToPatient(this RegisterPatientDto dto)
    {
        var patient = new Patient
        {
            Name = dto.FullName,
            Email = dto.Email,
            Phone = dto.PhoneNumber,
            User = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = "Patient",
                IsActive = true
            }
        };
        patient.UserId = patient.User.Id;
        
        patient.CreatedAt = DateTime.Now;
        return patient;
    }

    public static ReadPatientProfileDto ToReadPatientProfileDto(this Patient patient)
    {
        return new ReadPatientProfileDto(
            patient.Name ?? "",
            patient.Gender ?? "",
            patient.DateOfBirth.ToShortDateString(),
            patient.Phone,
            patient.Email,
            patient.Address ?? "",
            patient.City ?? "",
            patient.Height ?? 0.0f,
            patient.Weight ?? 0.0f,
            patient.BloodGroup ?? "",
            patient.ProfileInfo ?? ""
        );
    }
    

}