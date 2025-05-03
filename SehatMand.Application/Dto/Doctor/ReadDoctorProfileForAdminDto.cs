namespace SehatMand.Application.Dto.Doctor;

public record ReadDoctorProfileForAdminDto(
    string Id,
    string Name,
    string Email,
    string Contact,
    string Address,
    DateTime JoiningDate,
    string Speciality,
    string Status,
    string ProfilePicturePath,
    int UpcomingAppointmentsCount,
    int CompletedAppointmentsCount,
    int CancelledAppointmentsCount,
    float Rating,
    int PatientCount
    );