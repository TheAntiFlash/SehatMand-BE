namespace SehatMand.Application.Dto.Doctor;

public record ReadDoctorProfileDto(
    string Id,
    string Name,
    string City,
    string Speciality,
    List<string> Degrees,
    string Email,
    string Phone,
    float Rating,
    string ClinicId,
    string Address,
    string ProfileInfo,
    List<ReadDoctorDailyAvailability> Availability
    );