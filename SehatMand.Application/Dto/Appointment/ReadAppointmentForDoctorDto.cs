namespace SehatMand.Application.Dto.Appointment;

public record ReadAppointmentForDoctorDto(
    string Id,
    string PatientName,
    bool IsOnline,
    string Status,
    string Date,
    string Time,
    bool CanJoin,
    string CreatedAt
    );