using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Authentication;

public record UpdatePasswordDto(
    [Required(ErrorMessage = "Old password is required")]
    string OldPassword,
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    string NewPassword
    );