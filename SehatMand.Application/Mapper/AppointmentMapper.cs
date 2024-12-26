using System.Globalization;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Billing;
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
        var canJoin = (e.appointment_date <= DateTime.Now && e.appointment_date.AddHours(1) >= DateTime.Now) && e.status == "scheduled";
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
        var canJoin = (e.appointment_date <= DateTime.Now && e.appointment_date.AddHours(1) >= DateTime.Now) && e.status == "scheduled";
        List<(bool, string)?> cantCompleteReasons =
        [
            (e.status == "completed", "Appointment has already been completed"),
            (e.status != "scheduled", "Appointment can only be completed if scheduled"),
            (e.appointment_date > DateTime.Now, "Appointment time has not arrived yet"),
            (e.appointment_date.AddDays(1) < DateTime.Now, "Appointment time has passed, please contact admin 0348-5776651"),
            (!e.DidDoctorJoin, "Please join the appointment first"),
            (!e.DidPatientJoin, "Patient has not joined the appointment yet"),
            (e.Documents.Count == 0, "Please upload your notes first")
        ];
        
        var cantCompleteReason3 = e.appointment_date.AddDays(1) < DateTime.Now;
        var cantCompleteReason4 = e.Documents.Count == 0;
        var canComplete = !cantCompleteReasons.Any(x => x!.Value.Item1); 
            
        return new ReadAppointmentForDoctorDto(
            e.id,
            e.patient?.Name ?? string.Empty,
            e.online,
            e.status,
            e.appointment_date.Date.ToLongDateString(),
            e.appointment_date.ToString("h:mm tt"),
            canJoin,
            canComplete,
            cantCompleteReasons.FirstOrDefault(a => a!.Value.Item1)?.Item2,
            e.created_at.ToString("dd/MM/yyyy h:mm tt")
        );
    }
    
    public static ReadBillingInfoDto ToReadBillingInfoDto(this Appointment e)
    {
        return new ReadBillingInfoDto(
            e.Billing.First().id,
            e.id,
            e.patient?.Name ?? string.Empty,
            e.Billing.First().amount,
            e.Billing.First().transaction_date
            );
    }
}