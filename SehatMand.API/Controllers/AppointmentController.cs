using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Notification;

namespace SehatMand.API.Controllers;

/// <summary>
///This controller is responsible for handling all the appointment related requests.
/// It includes the following functionalities:
/// - Get all appointments for a patient
/// - Request an appointment
/// - Get all appointments for a doctor
/// - Update appointment status for a doctor 
/// </summary>
/// <param name="appointmentRepo"></param>
/// <param name="logger"></param>
/// <param name="notificationServ"></param>
[ApiController]
[Route("api/appointment")]
public class AppointmentController(
    IAppointmentRepository appointmentRepo,
    ILogger<AppointmentController> logger,
    IPushNotificationService notificationServ
    ): ControllerBase
{
    /// <summary>
    /// Get all appointments for a patient
    /// </summary>
    /// <param name="query">query by status</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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
    
    /// <summary>
    /// Request an appointment
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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
            await notificationServ.SendPushNotificationAsync(
                "New Appointment Request",
                "New Appointment Request",
                $"You have a new appointment request on {appointment.appointment_date.ToLongDateString()} at {appointment.appointment_date.ToString("h:mm tt")}",
                [appointment.doctor?.UserId?? ""], NotificationContext.APPOINTMENT_REQUEST);
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
    
    /// <summary>
    /// Get all appointments for a doctor
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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
    
    
    /// <summary>
    /// Update appointment status for a doctor
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="appointmentId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
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