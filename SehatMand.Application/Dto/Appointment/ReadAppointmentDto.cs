namespace SehatMand.Application.Dto.Appointment;

public record ReadAppointmentDto(
    string Id,
    string DoctorName,
    string MedicalField,
    DateTime Date,
    TimeSpan Time
    );