using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class AppointmentRepository(SmDbContext context, IPaymentService stripeServ, ILogger<AppointmentRepository> logger): IAppointmentRepository
{
    public async Task<(Appointment, string)> CreateAppointmentAsync(Appointment? appointment, string patientUid)
    {
        var patientId = await context.Patient
            .Where(p => p.UserId == patientUid)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        if( patientId == null) throw new Exception("Logged In Patient not found");
        
        appointment.patient_id = patientId;
        
        //validating appointment time
        var doctor = await context.Doctor
            .Include(d => d.DoctorDailyAvailability)
            .FirstOrDefaultAsync(d => d.Id == appointment.doctor_id);
        if (doctor == null) throw new Exception("Doctor not found");
        var availability = doctor.DoctorDailyAvailability
            .FirstOrDefault(a => a.day_of_week == (int)appointment.appointment_date.DayOfWeek);
        if (availability == null) throw new Exception("Doctor not available on this day");
        if (availability.availability_start > appointment.appointment_date.TimeOfDay ||
            availability.availability_end < appointment.appointment_date.TimeOfDay)
        {
            throw new Exception("Doctor not available at this time");
        }
        if (doctor.DoctorPaymentId == null) throw new Exception("Doctor payment account not found");
        
        var intent = await stripeServ.CreatePaymentIntentAsync(5000, doctor.DoctorPaymentId, appointment.id);
        appointment.paymentIntentId = intent.Id;
        await context.Appointment.AddAsync(appointment);
        await context.SaveChangesAsync();
        var appointmentSaved = await context.Appointment
            .Include(a => a.doctor)
            .ThenInclude(d => d.Qualifications)
            .FirstOrDefaultAsync(a => a.id == appointment.id);
        return (appointmentSaved!, intent.ClientSecret);
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(string patientUid, string? statusQuery)
    {
        var patientId = await context.Patient
            .Where(p => p.UserId == patientUid)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        if( patientId == null) throw new Exception("Logged In Patient not found");
        
        var dbQuery = context.Appointment
            .Include(a => a.doctor)
            .ThenInclude(d => d.Qualifications)
            .Where(a => a.patient_id == patientId);

        if (!string.IsNullOrWhiteSpace(statusQuery))
        {
            dbQuery = dbQuery.Where(a => a.status == statusQuery);
        }
        var appointments = await dbQuery.ToListAsync();
        return appointments;
    }

    public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorUid, string? queryStatus)
    {
        var doctorId = await context.Doctor
            .Where(p => p.UserId == doctorUid)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        if(doctorUid == null) throw new Exception("Logged In Doctor not found");
        
        var dbQuery = context.Appointment
            .Include(a => a.patient)
            .Where(a => a.doctor_id == doctorId);

        if (!string.IsNullOrWhiteSpace(queryStatus))
        {
            dbQuery = dbQuery.Where(a => a.status == queryStatus);
        }
        var appointments = await dbQuery.ToListAsync();
        return appointments;
    }

    public async Task<Appointment> UpdateAppointmentStatusAsync(string appointmentId, string id, string dtoStatus)
    {
        var appointment = context.Appointment
            .Include(a => a.doctor)
            .Include(a => a.patient)
            .FirstOrDefault(a => a.id == appointmentId);
        if (appointment == null) throw new Exception("Appointment not found");

        var scheduledAtTime = await context.Appointment
            .Where(a => a.doctor_id == appointment.doctor_id &&
                (a.appointment_date == appointment.appointment_date ||
                 (a.appointment_date < appointment.appointment_date &&
                  a.appointment_date.AddMinutes(59) >= appointment.appointment_date)) &&
                a.status == "scheduled" || a.status == "completed").AnyAsync();
       
        if (scheduledAtTime && dtoStatus == "scheduled") throw new Exception("Doctor is already scheduled at this time");
       
        if (appointment.doctor == null || appointment.doctor.UserId != id) 
            throw new Exception("Unauthorized");
       
        /*logger.LogError(appointment.status);
        logger.LogError((appointment.status != "pending").ToString());
        logger.LogError((appointment.status != "scheduled").ToString());
        logger.LogError((appointment.status != "pending" && appointment.status != "scheduled").ToString());*/
           
        if (appointment.status == "pending")
            if(dtoStatus != "scheduled" && dtoStatus != "rejected")
                throw new Exception("Appointment is already completed, cancelled, or rejected");
       
        if (appointment.status == "scheduled")
            if(dtoStatus != "cancelled")
                throw new Exception("Appointment is already completed, cancelled, or rejected");
       
        appointment.status = dtoStatus;
        await context.SaveChangesAsync();
        return appointment;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
    {
        return await context.Appointment.FirstOrDefaultAsync(a => a.id == appointmentId);  
    }

    public async Task AddReviewAsync(Review review, string patientId)
    {
        var appointment = await context.Appointment.Include(a => a.Review).FirstOrDefaultAsync(a => a.id == review.appointment_id);
        if (appointment == null) throw new Exception("Appointment not found");
        if (appointment.patient_id != patientId) throw new Exception("Unauthorized");
        if (appointment.status != "completed")
        {
            throw new Exception("Appointment is not completed yet");
        }
        if(appointment.Review.Count > 0) throw new Exception("Review already added");
        await context.Review.AddAsync(review);
        await context.SaveChangesAsync();
    }
}