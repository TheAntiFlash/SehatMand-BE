namespace SehatMand.Application.Dto.Appointment;

public record ReadAppointmentDto(
    string Id,
    string DoctorName,
    string MedicalField,
    string Status,
    string Date,
    string Time,
    bool CanJoin,
    string CreatedAt
    );