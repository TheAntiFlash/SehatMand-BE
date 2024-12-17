namespace SehatMand.Application.Dto.Doctor;

public record ReadNearestDoctorDto(
    string Id,
    string Name,
    string Speciality,
    string ProfileUrl,
    List<string> Degrees,
    string WorkingStart,
    string WorkingEnd,
    string Email,
    string Phone,
    float Rating
    );