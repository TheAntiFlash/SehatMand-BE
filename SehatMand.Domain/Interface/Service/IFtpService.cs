namespace SehatMand.Domain.Interface.Service;

public interface IFtpService
{
    //Task<string> UploadFileAsync(byte[] fileData, string fileName);

    Task<string> SaveFileLocallyAsync(byte[] fileData, string fileName, string folderPath);
}
