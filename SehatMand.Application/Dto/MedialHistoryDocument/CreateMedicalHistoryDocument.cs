using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DateOnly = SehatMand.Application.Dto.Utils.DateOnly;

namespace SehatMand.Application.Dto.MedialHistoryDocument;

public record CreateMedicalHistoryByPatientDocument(
    [Required]
    string Name,
    [Required]
    string Description,
    [Required]
    DateOnly RecordDate,
    [Required]
    IFormFile File
    );