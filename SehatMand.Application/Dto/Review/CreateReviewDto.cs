using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Review;

public record CreateReviewDto(
    [Required]
    [Range(0,10, ErrorMessage = "Rating must be between 0 and 10")]
    int Rating,
    [Required]
    string Feedback
    );