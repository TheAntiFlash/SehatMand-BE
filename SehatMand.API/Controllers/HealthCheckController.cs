using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/healthcheck")]
public class HealthCheckController(IAudioRebuilderService audioRebuilderService): ControllerBase
{
    /// <summary>
    /// Health check endpoint
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> HealthCheck()
    {
        var path = await audioRebuilderService.ProcessAudioAsync("41113444ab4037ca9a1854bb9677eca2", "fbc11754-8c10-420b-8971-8d2db8b85f82");
        return Ok(new
        {
            Status = "Healthy",
            path
        });
    }
}