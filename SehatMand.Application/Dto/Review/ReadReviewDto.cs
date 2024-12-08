namespace SehatMand.Application.Dto.Review;

public record ReadReviewDto(
    string Id,
    float Rating,
    string Feedback
    );