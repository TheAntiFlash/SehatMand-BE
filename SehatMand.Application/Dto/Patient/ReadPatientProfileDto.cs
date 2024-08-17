namespace SehatMand.Application.Dto.Patient;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

public record ReadPatientProfileDto(
    string Name, 
    string Gender,
    string DateOfBirth,
    string PhoneNumber,
    string Email,
    string Address,
    string City,
    float Height,
    float Weight,
    string BloodGroup,
    string ProfileInfo
    );