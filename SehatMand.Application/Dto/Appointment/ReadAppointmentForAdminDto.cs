namespace SehatMand.Application.Dto.Appointment;

public record ReadAppointmentForAdminDto(
    string Id,
    string PatientId,
    string DoctorId,
    string DoctorName,
    string PatientName,
    string? DoctorProfilePictureUrl,
    DateTime Date,
    DateTime CreatedAt,
    string Status,
    string? RecordingUrl,
    string TransactionAmount,
    float? Review,
    string? ReviewComment
    );