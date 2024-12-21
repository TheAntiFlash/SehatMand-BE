using SehatMand.Application.Dto.Review;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class ReviewMapper
{
    public static Review ToReview(this CreateReviewDto dto, string appointmentId)
    {
        return new Review
        {
            appointment_id = appointmentId,
            rating = dto.Rating,
            feedback = dto.Feedback
        };
    }
   
    public static ReadReviewDto ToReadReviewDto(this Review review)
    {
        return new ReadReviewDto(
            review.id,
            review.appointment?.patient?.Name ?? string.Empty,
            review.appointment?.patient_id ?? string.Empty,
            review.rating / 2f,
            review.feedback,
            review.created_at
        );
    }
}