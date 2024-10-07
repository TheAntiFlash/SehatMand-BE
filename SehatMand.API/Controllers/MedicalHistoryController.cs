/*using Microsoft.AspNetCore.Mvc;
using SehatMand.Domain.Interface.Repository;

namespace SehatMand.API.Controllers;

/// <summary>
/// Medical history controller
/// </summary>
/// <param name="repo"></param>
/// <param name="logger"></param>
[ApiController]
[Route("api/medical-history")]
public class MedicalHistoryController(IMedicalHistoryRepository repo, ILogger<MedicalHistoryController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddMedicalHistory([FromForm])
    {
        return Ok();
    }
}*/