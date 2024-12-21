using System.Globalization;
using SehatMand.Application.Dto;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Doctor;
using SehatMand.Application.Dto.PmcDoctor;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class DoctorMapper
{
    public static Doctor? ToDoctor(this RegisterDoctorDto dto, PmcDoctor pmc)
    {
        var doctorUser = new User
        {
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Doctor",
            IsActive = false,
            
        };
        var availability = new List<DoctorDailyAvailability>();
        for (int i = 0; i < 7; i++)
        {
            availability.Add(
                new DoctorDailyAvailability()
                {
                    day_of_week = i,
                    availability_start = new TimeSpan(9, 0, 0), 
                    availability_end = new TimeSpan(17, 0 , 0),
                    created_at = DateTime.Now,
                    created_by = doctorUser.Id,
                }
                );
        }
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
            SpecialityId = dto.SpecialityId,
            ClinicId = null,
            Address = dto.Address,
            ProfileInfo = "",
            City = dto.City,
            DoctorDailyAvailability = availability
        };
    }
    
    public static ReadNearestDoctorDto ToReadNearestDoctorDto(this Doctor doctor)
    {
        return new ReadNearestDoctorDto(
            doctor.Id,
            doctor.Name,
            doctor.Speciality?.Value ?? "N/A",
            $"doctor/doctor.Id",
            doctor.Qualifications.Select(q => q.Degree).ToList(),
            doctor.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week! == (int?)DateTime.Now.DayOfWeek)?.availability_start.ToString() ?? "unassigned",
            doctor.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week! == (int?)DateTime.Now.DayOfWeek)?.availability_end.ToString() ?? "unassigned",
            doctor.Email,
            doctor.Phone,
            (float)doctor.Appointment
                .Where(a => a.Review.Count > 0)
                .Select(a => a.Review.Average(r => r.rating)).FirstOrDefault()/2f
        );
    }


    public static ReadDoctorProfileDto ToReadDoctorProfileDto(this Doctor d)
    {
        var currDay = DateTime.Now.Date;
        var availabilities = new List<ReadDoctorDailyAvailability>();
        for (var i = 0; i < 14; i++)
        {
            var availability = new ReadDoctorDailyAvailability(
                currDay.DayOfWeek.ToString(),
                currDay.ToShortDateString(),
                d.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week == (int)currDay.DayOfWeek)?.availability_start.ToString() ?? "unassigned",
                d.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week == (int)currDay.DayOfWeek)?.availability_end.ToString() ?? "unassigned",
                d.Appointment.Where(a => a.appointment_date.Date == currDay.Date && a.status == "scheduled").Select(t => t.appointment_date.ToString("HH:mm")).ToList()
                );
            availabilities.Add(availability);
            currDay = currDay.AddDays(1);
        }
        return new ReadDoctorProfileDto(
            d.Id,
            d.Name,
            d.City?? "N/A",
            d.Speciality?.Value ?? "N/A",
            d.Qualifications.Select(q => q.Degree).ToList(),
            d.Email,
            d.Phone,
            (float)d.Appointment
                .Where(a => a.Review.Count > 0)
                .Select(a => a.Review.Average(r => r.rating)).FirstOrDefault()/2f,
            d.ClinicId?? "N/A",
            d.Address?? "N/A",
            d.ProfileInfo?? "N/A",
            availabilities
        );
    }

    public static DoctorAvailability ToDoctorAvailability(this DoctorAvailabilityDto dto)
    {
        return new DoctorAvailability(
            dto.DayOfWeek,
            dto.StartTimeInternal,
            dto.EndTimeInternal
            );
    }
}