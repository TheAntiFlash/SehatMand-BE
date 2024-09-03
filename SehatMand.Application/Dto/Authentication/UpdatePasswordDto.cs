using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Authentication;

public record UpdatePasswordDto(
    [Required(ErrorMessage = "Old password is required")]
    string OldPassword,
    [Required(ErrorMessage = "New password is required")]
    [MinLength(4, ErrorMessage = "Password must be at least 4 characters long")]
    string NewPassword
    );