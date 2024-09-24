using System.Globalization;
using SehatMand.Application.Dto;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Dctor;
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
            RegistrationDate = DateTime.ParseExact(pmc.Data.RegistrationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
            LicenseExpiry = DateTime.ParseExact(pmc.Data.ValidUpto,"dd/MM/yyyy",CultureInfo.InvariantCulture),
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
            City = dto.City,
            DoctorDailyAvailability = new List<DoctorDailyAvailability>
            {
                new DoctorDailyAvailability
                {
                    day_of_week = 1,
                    availability_start = new DateTime(),
                    availability_end = null,
                    created_at = null,
                    modified_at = null,
                    created_by = null,
                    created_byNavigation = null,
                    doctor = null
                }
            }
            
        };
    }
    
    public static ReadNearestDoctorDto ToReadNearestDoctorDto(this Doctor doctor)
    {
        return new ReadNearestDoctorDto(
            doctor.Id,
            doctor.Name,
            doctor.Qualifications.Select(q=>q.Speciality).ToList(),
            $"doctor/doctor.Id",
            doctor.Qualifications.Select(q => q.Degree).ToList(),
            doctor.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week! == (int?)DateTime.Now.DayOfWeek)?.availability_start.ToString() ?? "unassigned",
            doctor.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week! == (int?)DateTime.Now.DayOfWeek)?.availability_end.ToString() ?? "unassigned",
            doctor.Email,
            doctor.Phone,
            4.8f
        );
    }
}