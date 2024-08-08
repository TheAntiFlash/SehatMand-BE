using System.ComponentModel.DataAnnotations;
using SehatMand.Application.Enum;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Application.Dto.Patient;

public record CompletePatientProfileDto(
    [Required(ErrorMessage = "Address is required")]
    string Address,
    [Required(ErrorMessage = "City is required")]
    string City,
    [Required(ErrorMessage = "Province is required")]
    [Range(21,107, ErrorMessage = "Height must be between 21 and 107 inches")]
    float HeightInInches,
    [Required(ErrorMessage = "Weight is required")]
    [Range(20, 635, ErrorMessage = "Weight must be between 1 and 1000 kg")]
    float Weight,
    [Required]
    [EnumDataType(typeof(Gender))]
    string Gender,
    [Required(ErrorMessage = "Blood Group is required")]
    [EnumDataType(typeof(BloodGroup))]
    string BloodGroup,
    DateOnly DateOfBirth
    );