namespace SehatMand.Application.Dto.Appointment;

public record ReadAppointmentCountByMonthDto(
    string Month,
    int AppointmentCount
    );