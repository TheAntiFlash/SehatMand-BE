using SehatMand.Application.Dto.Appointment;

namespace SehatMand.Application.Dto.Admin;

public record ReadDashboardDataDto(
    int TotalDoctors,
    int TotalPatients,
    int TotalAppointments,
    int TotalForumPosts,
    List<ReadAppointmentCountByMonthDto> AppointmentsByMonth
    );