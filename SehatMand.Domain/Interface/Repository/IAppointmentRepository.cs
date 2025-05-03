using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IAppointmentRepository
{
    public Task<(Appointment, string ClientSecret)> CreateAppointmentAsync(Appointment? appointment, string patientUid);
    
    public Task<List<Appointment>> GetAppointmentsAsync(string patientUid, string? statusQuery,
        bool? queryShowPastAppointments);
    Task<List<Appointment>> GetDoctorAppointmentsAsync(string doctorUid, string? queryStatus,
        bool? queryShowPastAppointments);
    public Task<Appointment> UpdateAppointmentStatusAsync(string appointmentId, string id, string dtoStatus);
    public Task<Appointment?> GetAppointmentByIdAsync(string appointmentId);
    Task<Appointment> AddReviewAsync(Review review, string patientId);
    
    Task DoctorJoinedAppointment(string appointmentId);
    Task PatientJoinedAppointment(string appointmentId);
    
    Task AddRecordingDetails(string appointmentId, string resourceId, string startId);
    Task<int> GetTotalAppointmentsAsync();
    Task<Dictionary<string, int>> GetAppointmentsByMonthAsync();
}