using Microsoft.AspNetCore.Mvc;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.API.Controllers;

/// <summary>
/// </summary>
/// <param name="service"></param>
[ApiController]
[Route("api/[controller]")]
public class AgoraController(IAgoraService service): ControllerBase
{
    
}