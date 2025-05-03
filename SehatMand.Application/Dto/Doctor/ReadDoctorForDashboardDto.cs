namespace SehatMand.Application.Dto.Doctor;

public record ReadDoctorForDashboardDto(
    string DoctorId,
    string ImageUrl,
    string Name,
    string Email,
    string Speciality,
    int PatientCount,
    int AppointmentCount,
    DateTime JoinDate,
    float Rating,
    string Status
    );