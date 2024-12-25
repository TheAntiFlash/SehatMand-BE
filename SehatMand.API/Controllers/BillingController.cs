using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Domain.Interface.Repository;
using Stripe;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController(
    IAppointmentRepository appointmentRepo,
    ILogger<BillingController> logger
    ): ControllerBase
{
   
    /// <summary>
    /// Get total balance
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("balance")]
    public async Task<IActionResult> GetReceivedDetails()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointments = await appointmentRepo.GetDoctorAppointmentsAsync(id, "completed", true);
            var total = appointments.Where(a => a.Billing.Count > 0).Sum(a => a.Billing.First().amount);
            
            return Ok(new {Balance = total});


        }
        catch (Exception e)
        {
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "An error occurred while fetching received details:", "" +e.Message));
        }
        
    }
    
}