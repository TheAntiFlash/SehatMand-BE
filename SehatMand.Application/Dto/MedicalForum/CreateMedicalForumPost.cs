using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.MedicalForum;

public record CreateMedicalForumPost(
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(255, ErrorMessage = "Title must be less than 255 characters")]
    string Title,
    [Required(ErrorMessage = "Content is required")]
    [MaxLength(500, ErrorMessage = "Content must be less than 500 characters")]
    string Content
    );