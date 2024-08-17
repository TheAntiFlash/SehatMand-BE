namespace SehatMand.Application.Dto.Dctor;

public record ReadNearestDoctorDto(
    
    string Name,
    List<string> Specialities,
    List<string> Degrees,
    string Email,
    string Phone,
    string Address
    );