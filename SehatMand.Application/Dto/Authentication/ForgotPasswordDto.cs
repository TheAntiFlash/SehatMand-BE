using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Authentication;

public record ForgotPasswordDto(
    [Required]
    string Email,
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")] 
    string NewPassword,
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")] 
    string ConfirmPassword,
    [Required]
    string Otp
    
    );