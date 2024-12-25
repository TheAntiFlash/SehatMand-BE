using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Mapper;
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
            logger.LogError("{name} Error while fetching received details. Error: " + e.Message, "Billing");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "An error occurred while fetching received details:", "" +e.Message));
        }
    }
    
    /// <summary>
    /// Get billing history
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpGet]
    [Route("history")]
    public async Task<IActionResult> GetBillingHistory()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointments = await appointmentRepo.GetDoctorAppointmentsAsync(id, "completed", true);
            var billing = appointments.Select(a => a.ToReadBillingInfoDto()).ToList();
            return Ok(billing);
        }
        catch (Exception e)
        {
            logger.LogError("{name} Error while fetching billing history. Error: " + e.Message, "Billing");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "An error occurred while fetching billing history:", "" +e.Message));
        }
    }
    
}