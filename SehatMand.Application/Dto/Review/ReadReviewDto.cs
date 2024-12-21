namespace SehatMand.Application.Dto.Review;

public record ReadReviewDto(
    string Id,
    string PatientName,
    string PatientId,
    float Rating,
    string Feedback,
    DateTime CreatedAt
    );