using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using ThirdParty.Json.LitJson;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Application.Dto.Authentication;

public record RegisterDoctorDto(
    [Required] [EmailAddress]
    string email,
    [Phone]
    [Required]
    [RegularExpression(@"^(\+92|0)\d{10}$", ErrorMessage = "Invalid phone number")]
    string phoneNumber,
    [Required] [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")] 
    string password,
    [Required] 
    string confirmPassword,
    [Required] 
    string address,
    [Required]
    string city,
    [Required]
    string specialityId,
    [Required] 
    string pmcRegistrationCode,
    [Required]
    IFormFile profilePicture
)
{
    public void Validate()
    {
        if (password != confirmPassword)
        {
            throw new Exception("Passwords do not match");
        }
        //if less than 2MB
        if (profilePicture.Length > 2097152L)
        {
            throw new Exception("Profile picture size should be less than 2MB");
        }
        if (profilePicture.ContentType != "image/jpeg" &&
            profilePicture.ContentType != "image/png" &&
            profilePicture.ContentType != "image/jpg")
        {
            throw new Exception("Profile picture should be in jpeg/jpg or png format");
        }
        
        
    }

    
}