namespace SehatMand.Application.Dto.MedialHistoryDocument;

public record ReadMedicalHistoryDoctorInfo(
    string? Diagnosis,
    List<string>? Symptoms,
    string? Comments
    );