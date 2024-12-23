using SehatMand.Application.Dto.Review;

namespace SehatMand.Application.Dto.Doctor;

public record ReadDoctorProfileDto(
    string Id,
    string Name,
    string? ProfilePicturePath,
    string City,
    string Speciality,
    List<string> Degrees,
    string Email,
    string Phone,
    float Rating,
    string ClinicId,
    string Address,
    string ProfileInfo,
    List<ReadReviewDto> Reviews,
    List<ReadDoctorDailyAvailability> Availability
    );