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
            e.appointment_date,
            e.patient?.Name ?? string.Empty,
            e.Review.FirstOrDefault()?.rating /2f ?? 0f,
            e.Billing.First().amount,
            e.Billing.First().transaction_date
            );
    }

    public static List<ReadAppointmentCountByMonthDto> ToReadAppointmentCountByMonthDto(this Dictionary<string, int> e)
    {
        return e.Select(i => 
            new ReadAppointmentCountByMonthDto(
                DateTime.ParseExact(i.Key, "MMMM yyyy", CultureInfo.InvariantCulture).ToString("MMMM yy"),
                i.Value
            )).ToList();
    }

    public static ReadAppointmentForAdminDto ToReadAppointmentForAdminDto(this Appointment a)
    {
        return new ReadAppointmentForAdminDto(
            a.id,
            a.patient_id,
            a.doctor_id,
            a.doctor?.Name ?? string.Empty,
            a.patient?.Name ?? string.Empty,
            a.doctor?.ProfilePictureUrl ?? string.Empty,
            a.appointment_date,
            a.created_at,
            a.status,
            a.RecordedSessions?.FirstOrDefault()?.session_link ?? string.Empty,
            a.Billing.FirstOrDefault()?.amount.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
            a.Review.FirstOrDefault()?.rating / 2f ?? 0f,
            a.Review.FirstOrDefault()?.feedback ?? string.Empty
            );
    }
}