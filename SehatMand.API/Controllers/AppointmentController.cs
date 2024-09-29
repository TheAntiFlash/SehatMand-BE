using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/appointment")]
public class AppointmentController(
    IAppointmentRepository appointmentRepo,
    ILogger<AppointmentController> logger
    ): ControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAppointments([FromQuery] QueryAppointmentStatus? query)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointments = await appointmentRepo.GetAppointmentsAsync(id, query?.Status);

            return Ok(appointments.Select(a => a.ToReadAppointmentDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get appointments");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get appointments",
                e.Message
            ));
        
        }
    }
    
    [Authorize]
    [HttpPost]
    [Route("request")]
    public async Task<IActionResult> RequestAppointment([FromBody] CreateAppointmentDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointment = await appointmentRepo.CreateAppointmentAsync(dto.ToAppointment(), id);
            return Ok(appointment.ToReadAppointmentDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to create appointment");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to create appointment",
                e.Message
            ));
        
        }
    }
    
    [Authorize]
    [HttpGet]
    [Route("doctor")]
    public async Task<IActionResult> GetDoctorAppointments([FromQuery] QueryAppointmentStatus? query)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointments = await appointmentRepo.GetDoctorAppointmentsAsync(id, query?.Status);
            return Ok(appointments.Select(a => a.ToReadAppointmentForDoctorDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get appointments");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get appointments",
                e.Message
            ));
        }
    }
    
    
    [Authorize]
    [HttpPatch]
    [Route("doctor/{appointmentId}/update-status")]
    public async Task<IActionResult> UpdateAppointmentStatus([FromBody] UpdateAppointmentStatusDto dto,
        [FromRoute] string appointmentId)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");

            await appointmentRepo.UpdateAppointmentStatusAsync(appointmentId, id, dto.Status);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update appointment status");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to update appointment status",
                e.Message
            ));
        }
    }
}