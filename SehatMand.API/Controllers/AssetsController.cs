using Microsoft.AspNetCore.Mvc;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.API.Controllers;

/// <summary>
/// Assets controller
/// </summary>
/// <param name="storageServ"></param>
[ApiController]
[Route("assets")]
public class AssetsController(IStorageService storageServ, IWebHostEnvironment env): ControllerBase
{
    /// <summary>
    /// Get doctor profile picture
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("doctor/profile-picture/{filename}")]
    public async Task<IActionResult> GetDoctorProfilePicture(string filename)
    {
        
        var basePath = Path.Join("doctor/","profile-picture");
        var filePath = Path.Join(basePath+'/', filename);
        var file = await storageServ.DownloadFileAsync(filePath);
        return File(file.ResponseStream, file.Headers.ContentType);
    }
    
    /// <summary>
    /// Get Appointment Audio
    /// </summary>
    /// <param name="filename"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("audios/{filename}")]
    public async Task<IActionResult> GetAudio(string filename)
    {
        var filePath = Path.Join(env.WebRootPath, "audios", filename);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        const string contentType = "audio/mpeg"; // or use GetContentType(filePath)
        return File(fileBytes, contentType, filename);

    }
}