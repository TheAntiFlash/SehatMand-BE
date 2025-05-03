using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class AwsService(IAmazonS3 s3Client): IStorageService
{
    private const string BucketName = "sehatmand";
    public async Task UploadFileAsync(IFormFile file, string fileName, string prefix)
    {
        await s3Client.EnsureBucketExistsAsync(BucketName);
        //if (!bucketExists) throw new Exception($"Bucket {BucketName} does not exist.");
        var request = new PutObjectRequest
        {
            BucketName = BucketName,
            ContentType = file.ContentType,
            Key = string.IsNullOrEmpty(prefix) ? fileName : $"{prefix?.TrimEnd('/')}/{fileName}{Path.GetExtension(file.FileName)}",
            InputStream = file.OpenReadStream()
        };
        request.Metadata.Add("Content-Type", file.ContentType);
        await s3Client.PutObjectAsync(request);
    }

    public async Task<GetObjectResponse> DownloadFileAsync(string filePath)
    {
        await s3Client.EnsureBucketExistsAsync(BucketName);
        // if (!bucketExists) throw new Exception($"Bucket {BucketName} does not exist.");
        var s3Object = await s3Client.GetObjectAsync(BucketName, filePath);
        if (s3Object == null) throw new Exception($"File {filePath} does not exist.");
        return s3Object;
    }
    
    public async Task DeleteFileAsync(string filePath)
    {
        await s3Client.EnsureBucketExistsAsync(BucketName);
        // if (!bucketExists) throw new Exception($"Bucket {BucketName} does not exist.");
        await s3Client.DeleteObjectAsync(BucketName, filePath);
    }
    
}