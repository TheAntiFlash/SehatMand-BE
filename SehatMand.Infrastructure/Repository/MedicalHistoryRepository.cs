using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class MedicalHistoryRepository(
    IStorageService awsService,
    SmDbContext context
): IMedicalHistoryRepository
{
    public async Task<string> AddMedicalHistoryDocumentAsync(MedicalHistoryDocument document, IFormFile file, string rootFolder)
    {
        var path = Path.Join("medical-history",$"{document.created_at:yyyy-MM-dd}--{document.patient_id}", rootFolder);
        await awsService.UploadFileAsync(file, document.id, path);
        await context.MedicalHistoryDocument.AddAsync(document);
        await context.SaveChangesAsync();
        return document.id;
    }

    public async Task<GetObjectResponse> GetMedicalHistoryDocumentByIdAsync(string id)
    {
        var document = await context.MedicalHistoryDocument.FirstOrDefaultAsync(h => h.id == id);
        if (document?.document_path == null) throw new Exception($"Document with id {id} does not exist or does not have a document associated with it.");
        
        return await awsService.DownloadFileAsync(document.document_path);
    }

    public async Task<List<MedicalHistoryDocument>> GetMedicalHistoryDocumentsByPatientIdAsync(string patientId)
    {
        var documents = await context.MedicalHistoryDocument.Where(h => h.patient_id == patientId).ToListAsync();
        return documents;
    }
}