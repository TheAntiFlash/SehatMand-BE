using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;
using Stripe;
using Review = SehatMand.Domain.Entities.Review;

namespace SehatMand.Infrastructure.Repository;

public class AppointmentRepository(SmDbContext context, IPaymentService stripeServ, ILogger<AppointmentRepository> logger): IAppointmentRepository
{
    public async Task<(Appointment, string ClientSecret)> CreateAppointmentAsync(Appointment? appointment,
        string patientUid)
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
        
        var intent = await stripeServ.CreatePaymentIntentAsync(200000, doctor.DoctorPaymentId, appointment.id);
        appointment.paymentIntentId = intent.Id;
        await context.Appointment.AddAsync(appointment);
        await context.SaveChangesAsync();
        var appointmentSaved = await context.Appointment
            .Include(a => a.doctor)
            .ThenInclude(d => d.Qualifications)
            .OrderByDescending(a => a.appointment_date)
            .FirstOrDefaultAsync(a => a.id == appointment.id);
        return (appointmentSaved!, intent.ClientSecret);
    }

    public async Task<List<Appointment>> GetAppointmentsAsync(string patientUid, string? statusQuery,
        bool? queryShowPastAppointments)
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

        if (queryShowPastAppointments == false)
        {
            dbQuery = dbQuery.Where(a => a.appointment_date.AddMinutes(60) >= DateTime.Now);
        }

        if (!string.IsNullOrWhiteSpace(statusQuery))
        {
            dbQuery = dbQuery.Where(a => a.status == statusQuery).OrderByDescending(a => a.appointment_date);
        }
        var appointments = await dbQuery.OrderByDescending(a => a.appointment_date)
            .ToListAsync();
        var asyncTasks = new List<Task>();
        foreach (var appointment in appointments)
        {
            if (appointment.appointment_date.AddDays(1) < DateTime.Now && (appointment is {status: "scheduled" or "pending" or "payment-pending"}))
            {
                asyncTasks.Add(appointment.CancelOrRejectAppointmentAsync());
            }
        }
        await Task.WhenAll(asyncTasks);
        await context.SaveChangesAsync();
        return appointments;
    }

    public async Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorUid, string? queryStatus,
        bool? queryShowPastAppointments)
    {
        var doctorId = await context.Doctor
            .Where(p => p.UserId == doctorUid)
            .Select(p => p.Id)
            .FirstOrDefaultAsync();
        if(doctorUid == null) throw new Exception("Logged In Doctor not found");
        
        var dbQuery = context.Appointment
            .Include(a => a.patient)
            .Include(a => a.Documents)
            .Include(a => a.Billing)
            .Where(a => a.doctor_id == doctorId);

        if (queryShowPastAppointments == false)
        {
            dbQuery = dbQuery.Where(a => a.appointment_date.AddMinutes(60) >= DateTime.Now);
        }
        
        if (!string.IsNullOrWhiteSpace(queryStatus))
        {
            dbQuery = dbQuery.Where(a => a.status == queryStatus);
        }
        var appointments = await dbQuery.OrderByDescending(a => a.appointment_date)
            .ToListAsync();
        var asyncTasks = new List<Task>();
        foreach (var appointment in appointments)
        {
           if (appointment.appointment_date.AddDays(1) < DateTime.Now && (appointment is {status: "scheduled" or "pending" or "payment-pending"}))
           {
               asyncTasks.Add(appointment.CancelOrRejectAppointmentAsync());
           }
        }
        await Task.WhenAll(asyncTasks);
        await context.SaveChangesAsync();
        return appointments;
    }

    public async Task<Appointment> UpdateAppointmentStatusAsync(string appointmentId, string id, string dtoStatus)
    {
        var appointment = context.Appointment
            .Include(a => a.doctor)
            .Include(a => a.patient)
            .Include(a => a.RecordedSessions)
            .FirstOrDefault(a => a.id == appointmentId);
        if (appointment == null) throw new Exception("Appointment not found");

        var scheduledAtTime = await context.Appointment
            .Where(a => (a.doctor_id == appointment.doctor_id) &&
                ((a.appointment_date == appointment.appointment_date) || (a.appointment_date < appointment.appointment_date && a.appointment_date.AddMinutes(59) >= appointment.appointment_date)) &&
                (a.status == "scheduled" || a.status == "completed")).AnyAsync();
        
        // var appointments = await context.Appointment.Where(a => a.doctor_id == appointment.doctor_id).ToListAsync();
        // appointments = appointments.Where(a => a.appointment_date == appointment.appointment_date ||
        //                                               (a.appointment_date < appointment.appointment_date &&
        //                                                a.appointment_date.AddMinutes(59) >=
        //                                                appointment.appointment_date)).ToList();
        // appointments = appointments.Where(a => a.status == "scheduled" || a.status == "completed").ToList();
        // var scheduledAtTime = appointments.Any();
       
        if (scheduledAtTime && dtoStatus == "scheduled") throw new Exception("Doctor is already scheduled at this time");
       
        if (appointment.doctor == null || appointment.doctor.UserId != id) 
            throw new Exception("Unauthorized");
       
        
        if (appointment.status == "pending")
        {
            if(dtoStatus != "scheduled" && dtoStatus != "rejected")
                throw new Exception("appointment can only be scheduled or rejected from pending state.");
            if (dtoStatus == "scheduled" && appointment.appointment_date < DateTime.Now)
            {
                await appointment.CancelOrRejectAppointmentAsync();
                await context.SaveChangesAsync();
                throw new Exception("Appointment time has passed and can not be scheduled");
            }
        }
       
        if (appointment.status == "scheduled")
            if(dtoStatus != "cancelled" && dtoStatus != "completed")
                throw new Exception("Appointment is already completed, cancelled, or rejected");

        if (dtoStatus == "completed")
        {
            if (appointment.status != "scheduled")
                throw new Exception("Appointment can only be completed from scheduled state");
            // if (appointment.appointment_date.AddMinutes(5) >= DateTime.Now ||
            //     appointment.appointment_date.AddHours(2) <= DateTime.Now)
            //     throw new Exception("Appointment time has passed or is not completed yet");
            
            var paymentIntentService = new PaymentIntentService();
            if (appointment.paymentIntentId == null) throw new Exception("Payment not found");
            var paymentIntent = await paymentIntentService.CaptureAsync(appointment.paymentIntentId);
            Console.WriteLine($"Payment captured: {paymentIntent.Status}");
            appointment.Billing = [new Billing
            {
                amount = 1800,
                status = "paid",
                transaction_date = appointment.created_at,
                created_at = DateTime.Now
            }];
        }
       
        appointment.status = dtoStatus;
        await context.SaveChangesAsync();
        return appointment;
    }   

    public async Task<Appointment?> GetAppointmentByIdAsync(string appointmentId)
    {
        return await context.Appointment
            .Include(a => a.patient)
            .Include(a => a.doctor)
            .FirstOrDefaultAsync(a => a.id == appointmentId);  
    }

    public async Task<Appointment> AddReviewAsync(Review review, string patientId)
    {
        var appointment = await context.Appointment.Include(a => a.patient).Include(a => a.doctor).Include(a => a.Review).FirstOrDefaultAsync(a => a.id == review.appointment_id);
        if (appointment == null) throw new Exception("Appointment not found");
        if (appointment.patient_id != patientId) throw new Exception("Unauthorized");
        // if (appointment.status != "completed")
        // {
        //     throw new Exception("Appointment is not completed yet");
        // }
        if(appointment.Review.Count > 0) throw new Exception("Review already added");
        await context.Review.AddAsync(review);
        await context.SaveChangesAsync();
        return appointment;
    }

    public async Task DoctorJoinedAppointment(string appointmentId)
    {
        var appointment = await context.Appointment.FirstOrDefaultAsync(a => a.id == appointmentId);
        if (appointment == null) throw new Exception("Appointment not found");
        appointment.DidDoctorJoin = true;
        await context.SaveChangesAsync(); 
    }

    public async Task PatientJoinedAppointment(string appointmentId)
    {
        var appointment = await context.Appointment.FirstOrDefaultAsync(a => a.id == appointmentId);
        if (appointment == null) throw new Exception("Appointment not found");
        appointment.DidPatientJoin = true;
        await context.SaveChangesAsync();
    }

    public Task AddRecordingDetails(string appointmentId, string resourceId, string startId)
    {
        
        var recordedSession = new RecordedSessions
        {
            appointment_id = appointmentId,
            resource_id = resourceId,
            start_id = startId
        };
        context.RecordedSessions.Add(recordedSession);
        return context.SaveChangesAsync();
    }

    public Task<int> GetTotalAppointmentsAsync()
    {
        return context.Appointment.CountAsync();
    }

    public async Task<Dictionary<string, int>> GetAppointmentsByMonthAsync()
    {
        var appointments = await context.Appointment
            .Where(a => a.appointment_date >= DateTime.Now.AddMonths(-12))
            .ToListAsync();
        return appointments.GroupBy(a => a.appointment_date.ToString("MMMM yyyy"))
            .ToDictionary(
                g => g.Key,         // MonthYear as key
                g => g.Count()      // Count as value
            );

    }
}