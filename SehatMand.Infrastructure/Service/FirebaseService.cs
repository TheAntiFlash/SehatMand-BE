using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class FirebaseService: IStorageService
{
    public Task UploadFileAsync(byte[] file, string fileName)
    {
        throw new NotImplementedException();
    }
}