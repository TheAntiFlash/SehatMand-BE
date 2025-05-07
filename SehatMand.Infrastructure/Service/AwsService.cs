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
        // await s3Client.EnsureBucketExistsAsync(BucketName);
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
        // await s3Client.EnsureBucketExistsAsync(BucketName);
        // if (!bucketExists) throw new Exception($"Bucket {BucketName} does not exist.");
        var s3Object = await s3Client.GetObjectAsync(BucketName, filePath);
        if (s3Object == null) throw new Exception($"File {filePath} does not exist.");
        return s3Object;
    }
    
    public async Task DeleteFileAsync(string filePath)
    {
        // await s3Client.EnsureBucketExistsAsync(BucketName);
        // if (!bucketExists) throw new Exception($"Bucket {BucketName} does not exist.");
        await s3Client.DeleteObjectAsync(BucketName, filePath);
    }

    public async Task<(string, string)> GetM3U8FileAsync(string startId)
    {
        var request = new ListObjectsV2Request()
        {
            BucketName = BucketName,
            Prefix = "recordings/" + startId,
            MaxKeys = 1
        };
        var response = await s3Client.ListObjectsV2Async(request);
        // if (response.S3Objects.Count == 0)
        // {
        //     throw new Exception($"No files found with prefix {startId}");
        // }
        var fileName = response.S3Objects[0].Key;
        var m3u8File = await s3Client.GetObjectAsync(BucketName, fileName);
        if (m3u8File == null)
        {
            throw new Exception($"File {fileName} does not exist.");
        }

        using var reader = new StreamReader(m3u8File.ResponseStream);
        return (fileName, await reader.ReadToEndAsync());
    }

    public async Task<MemoryStream> GetMergedTsStreamAsync(string m3U8Content, string m3U8FileName)
    {
        var tsKeys = m3U8Content
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Where(line => line.Trim().EndsWith(".ts"))
            .Select(ts => $"{Path.GetDirectoryName(m3U8FileName)?.Replace("\\", "/")}/{ts.Trim()}")
            .ToList();

        // 2. Download and merge TS files in memory
        var mergedTsStream = new MemoryStream();
        foreach (var tsKey in tsKeys)
        {
            await using var tsStream = (await s3Client.GetObjectAsync(BucketName,tsKey)).ResponseStream;
            await tsStream.CopyToAsync(mergedTsStream);
        }
        mergedTsStream.Position = 0;
        return mergedTsStream;
    }
    
}