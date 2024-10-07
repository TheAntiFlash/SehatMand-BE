using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IAppointmentRepository
{
    public Task<Appointment> CreateAppointmentAsync(Appointment appointment, string patientUid);
    
    public Task<List<Appointment>> GetAppointmentsAsync(string patientUid, string? statusQuery);
    Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorUid, string? queryStatus);
    public Task<Appointment> UpdateAppointmentStatusAsync(string appointmentId, string id, string dtoStatus);
}