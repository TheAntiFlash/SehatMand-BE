using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class FtpService : IFtpService
{
    
    public async Task<string> SaveFileLocallyAsync(byte[] fileData, string fileName, string folderPath) 
    {
        // Define the folder path to save the file
        // Ensure the folder exists, if not create it
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        // Define the full file path
        string filePath = Path.Combine(folderPath, fileName);

        // Write the file to the local folder
        await File.WriteAllBytesAsync(filePath, fileData);
        return filePath;
    }
}
