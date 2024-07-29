using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Authentication;

public record LoginDto(
    [Required]
    [EmailAddress]
    string Email,
    [Required]
    string Password
    );