using System.ComponentModel.DataAnnotations;
using SehatMand.Application.Enum;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Application.Dto.Patient;

public record UpdatePatientProfileDto(
    string? FullName,
    [EmailAddress]
    string? Email,
    [Phone]
    string? PhoneNumber,
    string? Address,
    string? City,
    float? Height,
    float? Weight,
    [EnumDataType(typeof(Gender))]
    string? Gender,
    [EnumDataType(typeof(BloodGroup))]
    string? BloodGroup,
    DateOnly? DateOfBirth,
    string? ProfileInfo
    );