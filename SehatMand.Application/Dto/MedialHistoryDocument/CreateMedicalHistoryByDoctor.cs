using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace SehatMand.Application.Dto.MedialHistoryDocument;

public record CreateMedicalHistoryByDoctor(
    [Required]
    string Name,
    [Required]
    string Description,
    [Required]
    List<string> Symptoms,
    [Required]
    string Diagnosis,
    [Required]
    string Comments,
    [Required]
    IFormFile File
    );