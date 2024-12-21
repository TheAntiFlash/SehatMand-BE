using System.Globalization;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class AppointmentMapper
{
    public static Appointment? ToAppointment(this CreateAppointmentDto dto)
    {
        
        
        return new Appointment
        {
            doctor_id = dto.DoctorId,
            online = dto.IsOnline,
            appointment_date = dto.InternalAppointmentTime,
            created_at = DateTime.Now
        };
    }

    public static ReadAppointmentDto ToReadAppointmentDto(this Appointment e)
    {
        var canJoin = e.appointment_date > DateTime.Now && e.appointment_date < DateTime.Now.AddHours(1);
        return new ReadAppointmentDto(
            e.id,
            e.doctor?.Name ?? string.Empty,
            e.doctor?.Qualifications[0].Speciality ?? string.Empty,
            e.status,
            e.appointment_date.Date.ToLongDateString(),
            e.appointment_date.ToString("h:mm tt"),
            canJoin,
            e.created_at.ToString("dd/MM/yyyy h:mm tt")
        );
    }
    
    public static ReadAppointmentForDoctorDto ToReadAppointmentForDoctorDto(this Appointment e)
    {
        var canJoin = e.appointment_date > DateTime.Now && e.appointment_date < DateTime.Now.AddHours(1);
        return new ReadAppointmentForDoctorDto(
            e.id,
            e.patient?.Name ?? string.Empty,
            e.online,
            e.status,
            e.appointment_date.Date.ToLongDateString(),
            e.appointment_date.ToString("h:mm tt"),
            canJoin,
            e.created_at.ToString("dd/MM/yyyy h:mm tt")
        );
    }
}