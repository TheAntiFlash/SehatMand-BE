namespace SehatMand.Application.Dto.MedialHistoryDocument;

public record ReadMedicalHistoryDocument(
    string Id,
    string Name,
    string Description,
    ReadMedicalHistoryDoctorInfo? DoctorsComments,
    DateTime RecordDate,
    DateTime CreatedAt
    );