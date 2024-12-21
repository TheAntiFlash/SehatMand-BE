using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Application.Dto.Authentication;

public record RegisterDoctorDto(
    [Required] [EmailAddress] 
    string Email,
    [Phone] 
    string PhoneNumber,
    [Required] [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")] 
    string Password,
    [Required] 
    string ConfirmPassword,
    [Required] 
    string Address,
    [Required]
    string City,
    [Required]
    string SpecialityId,
    [Required] 
    string PmcRegistrationCode
)
{
    public void ValidatePassword()
    {
        if (Password != ConfirmPassword)
        {
            throw new Exception("Passwords do not match");
        }
        
    }
}