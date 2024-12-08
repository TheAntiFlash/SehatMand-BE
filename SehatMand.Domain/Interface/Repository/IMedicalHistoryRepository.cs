using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IMedicalHistoryRepository
{
    Task<string> AddMedicalHistoryDocumentAsync(MedicalHistoryDocument document, IFormFile file, string rootFolder);
    Task<GetObjectResponse> GetMedicalHistoryDocumentByIdAsync(string id);
    Task<List<MedicalHistoryDocument>> GetMedicalHistoryDocumentsByPatientIdAsync(string patientId);
}