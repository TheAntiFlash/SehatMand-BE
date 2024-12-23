using SehatMand.Application.Dto.MedialHistoryDocument;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class MedicalHistoryMapper
{
    public static MedicalHistoryDocument ToHistoryDocument(this CreateMedicalHistoryByPatientDocument h,
        string patientId, string userId)
    {
        var documentId = Guid.NewGuid().ToString();
        var documentPath = Path.Join("medical-history",$"{patientId}", "patient", documentId);
        return new MedicalHistoryDocument
        {
            id = documentId,
            patient_id = patientId,
            document_path = documentPath,
            document_name = h.Name,
            document_description = h.Description,
            record_date = h.RecordDate.ToDateTime(),
            created_at = DateTime.Now,
            created_by = userId,
        };
    }
    
    public static ReadMedicalHistoryDocument ToReadMedicalHistoryDocument(this MedicalHistoryDocument h)
    {
        var doctorComments = h.doctors_comments != null || h.symptoms != null || h.diagnosed_disease != null
            ? new ReadMedicalHistoryDoctorInfo(h.diagnosed_disease, h.symptoms?.Split(',').ToList(), h.doctors_comments)
            : null;
        return new ReadMedicalHistoryDocument(
            h.id,
            h.document_name?? "No Name",
            h.document_description ?? "No Description",
            doctorComments,
            h.record_date,
            h.created_at
        );
    }

    public static MedicalHistoryDocument ToHistoryDocument(this CreateMedicalHistoryByDoctor h, string appointmentId, string patientId, string userId)
    {
        var documentId = Guid.NewGuid().ToString();
        var documentPath = Path.Join("medical-history",$"{patientId}", "doctors-notes", documentId);
        return new MedicalHistoryDocument
        {
            id = documentId,
            appointment_id = appointmentId,
            patient_id = patientId,
            document_path = documentPath,
            document_name = h.Name,
            document_description = h.Description,
            doctors_comments = h.Comments,
            diagnosed_disease = h.Diagnosis,
            symptoms = string.Join(',', h.Symptoms),
            record_date = DateTime.Now,
            created_at = DateTime.Now,
            created_by = userId,
        };  
    }
}