namespace SehatMand.Domain.Interface.Service;

public interface IStorageService
{
    public Task UploadFileAsync(byte[] file, string fileName);
}