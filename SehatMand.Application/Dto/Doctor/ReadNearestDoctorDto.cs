namespace SehatMand.Application.Dto.Doctor;

public record ReadNearestDoctorDto(
    string Id,
    string Name,
    List<string> Specialities,
    string ProfileUrl,
    List<string> Degrees,
    string WorkingStart,
    string WorkingEnd,
    string Email,
    string Phone,
    float Rating
    );