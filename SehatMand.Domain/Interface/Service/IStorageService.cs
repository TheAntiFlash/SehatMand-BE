using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;

namespace SehatMand.Domain.Interface.Service;

public interface IStorageService
{
    public Task UploadFileAsync(IFormFile file, string fileName, string prefix);
    public Task<GetObjectResponse> DownloadFileAsync(string filePath);
    public Task DeleteFileAsync(string filePath);
    
}