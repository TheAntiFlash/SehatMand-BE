namespace SehatMand.Application.Dto.Doctor;

public record ReadDoctorDailyAvailability(
    string Day,
    string Date,
    string StartTime,
    string EndTime,
    List<String> AppointmentStartTimes
    );