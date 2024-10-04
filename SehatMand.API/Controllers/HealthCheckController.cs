using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/healthcheck")]
public class HealthCheckController: ControllerBase
{
    [HttpGet]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            Status = "Healthy"
        });
    }
}