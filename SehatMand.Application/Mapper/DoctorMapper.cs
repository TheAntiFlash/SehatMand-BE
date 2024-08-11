using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.PmcDoctor;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class DoctorMapper
{
    public static Doctor ToDoctor(this RegisterDoctorDto dto, PmcDoctor pmc)
    {
        var doctorUser = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Doctor",
            IsActive = true,
            
        };
        return new Doctor
        {
            Name = pmc.Data.Name,
            FatherName = pmc.Data.FatherName,
            RegistrationType = pmc.Data.RegistrationType,
            RegistrationDate = DateTime.Parse(pmc.Data.RegistrationDate),
            LicenseExpiry = DateTime.Parse(pmc.Data.ValidUpto),
            Qualifications = pmc.Data.Qualifications.Select(q=>
                new Qualification
                {
                    Id = Guid.NewGuid().ToString(),
                    Speciality = q.Speciality,
                    Degree = q.Degree,
                    University = q.University,
                    PassingYear = new DateTime(year: Convert.ToInt32(q.PassingYear), month: 1, day: 1)
                }
            ).ToList(),
            Email = dto.Email,
            Phone = dto.PhoneNumber,
            User = doctorUser,
            RegistrationId = pmc.Data.RegistrationNo,
            ApprovalStatus = "Approved",
            CreatedAt = DateTime.Now,
            Specialty = "",
            ClinicId = null,
            Address = dto.Address,
            ProfileInfo = "",
            
        };
    }
}