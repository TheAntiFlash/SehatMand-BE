using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.MedicalForum;

public record CreateMedicalForumCommentDto(
    [Required]
    [MaxLength(500, ErrorMessage = "Comment length should be less than 500 characters")]
    string Content
    );