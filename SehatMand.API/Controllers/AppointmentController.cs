using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Dto.Review;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Constants;
using SehatMand.Domain.Utils.Notification;
using Stripe;

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
    IPatientRepository patientRepo,
    IAgoraService agoraService,
    // IPaymentService StripeServ,
    ILogger<AppointmentController> logger,
    IPushNotificationService notificationServ,
    IAudioRebuilderService audioRebuilderService
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
            var appointments = await appointmentRepo.GetAppointmentsAsync(id, query?.Status, query?.ShowPastAppointments);

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
            if (dto.InternalAppointmentTime < DateTime.Now)
                return BadRequest(new ResponseDto("Invalid DateTime", "Appointment can only be scheduled in future."));
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var (appointment, clientSecret) = await appointmentRepo.CreateAppointmentAsync(dto.ToAppointment(), id);
            await notificationServ.SendPushNotificationAsync(
                "New Appointment Request",
                "New Appointment Request",
                $"You have a new appointment request on {appointment.appointment_date.ToLongDateString()} at {appointment.appointment_date.ToString("h:mm tt")}",
                [appointment.doctor?.UserId ?? ""], NotificationContext.APPOINTMENT_REQUEST);

            //return Ok(appointment.ToReadAppointmentDto());
            return Ok(new {appointment = appointment.ToReadAppointmentDto(), clientSecret
            });
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
            var appointments = await appointmentRepo.GetDoctorAppointmentsAsync(id, query?.Status, query?.ShowPastAppointments);
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

            var appointment = await appointmentRepo.UpdateAppointmentStatusAsync(appointmentId, id, dto.Status);
            if (appointment == null) throw new Exception("Unable to update appointment status");
            /*if (appointment.status == "scheduled")
            {
                await agoraService.ScheduleRoom(appointmentId, appointment.appointment_date);
            }*/
            await notificationServ.SendPushNotificationAsync(
                "New Appointment Request",
                "New Appointment Request",
                $"Your appointment with {appointment.doctor?.Name} on {appointment.appointment_date.ToLongDateString()} has been {appointment.status}",
                [appointment.patient?.UserId?? ""], NotificationContext.APPOINTMENT_REQUEST);
            
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
    
    /// <summary>
    /// Get agora rtc token
    /// </summary>
    /// <param name="appointmentId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("{appointmentId}/token")]
    public async Task<IActionResult> GetAgoraTokenAsync([FromRoute] string appointmentId)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var role = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
            var roleType = role == "Doctor" ? SehatMandConstants.DOCTOR : SehatMandConstants.PATIENT;
            logger.LogInformation(role + " " + roleType);
            var appointment = await appointmentRepo.GetAppointmentByIdAsync(appointmentId);
            
            var token = agoraService.GenerateRtcToken(roleType, appointmentId);
            if (role == "Doctor")
            {
                await appointmentRepo.DoctorJoinedAppointment(appointmentId);
                if (!appointment.DidPatientJoin)
                    await notificationServ.SendPushNotificationAsync(
                        $"Dr. {appointment.doctor?.Name} is waiting for you!",
                        "Join now!",
                        $"Your appointment with {appointment.doctor?.Name} has been started. Please join the call.",
                        [appointment.patient?.UserId?? ""], NotificationContext.APPOINTMENT_REQUEST);
            }
            else
            {
                await appointmentRepo.PatientJoinedAppointment(appointmentId);
                if (!appointment.DidDoctorJoin)
                    await notificationServ.SendPushNotificationAsync(
                        $"Your Patient {appointment.patient?.Name} is waiting for you!",
                        "Join now!",
                        $"Your appointment with {appointment.patient?.Name} has been started. Please join the call.",
                        [appointment.doctor?.UserId?? ""], NotificationContext.APPOINTMENT_REQUEST);
            }

            if (appointment.DidBothJoin)
            {
                try
                {
                    var (rid, sid) = await agoraService.Record(appointmentId);
                    await appointmentRepo.AddRecordingDetails(appointmentId, rid, sid);
                }
                catch (Exception e)
                {
                    logger.LogInformation(e.Message + " " + e.StackTrace);
                }
                
            }
            

            return Ok(token);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get agora token");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get agora token",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Complete appointment
    /// </summary>
    /// <param name="appointmentId"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPatch]
    [Route("{appointmentId}/complete")]
    public async Task<IActionResult> CompleteAppointment([FromRoute] string appointmentId)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var appointment = await appointmentRepo.GetAppointmentByIdAsync(appointmentId);
            // await agoraService.StopRecording(appointmentId,
            //     appointment.RecordedSessions.FirstOrDefault()?.resource_id ?? "",
            //     appointment.RecordedSessions.FirstOrDefault()?.start_id ?? "");
            Thread.Sleep(15000);
            var audioFilePath = await audioRebuilderService.ProcessAudioAsync(
                appointment.RecordedSessions.FirstOrDefault()?.start_id ?? "", appointmentId);
            
            
            await appointmentRepo.UpdateAppointmentStatusAsync(appointmentId, id, "completed", audioFilePath);
            var paymentIntentService = new PaymentIntentService();
            if (appointment.paymentIntentId == null) throw new Exception("Payment not found");


            await notificationServ.SendPushNotificationAsync(
                $"Leave a review?",
                $"How was your experience with Dr. {appointment.doctor?.Name}?\nLeave a review now!",
                $"Your appointment with {appointment.doctor?.Name} has been completed.",
                [appointment.patient?.UserId ?? ""], NotificationContext.APPOINTMENT_REQUEST);

            
            return Ok( new {
                audioFilePath
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get agora token");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get agora token",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Add review
    /// </summary>
    /// <param name="appointmentId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [Route("{appointmentId}/review")]
    public async Task<IActionResult> AddReview([FromRoute] string appointmentId, [FromBody] CreateReviewDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patientId = await patientRepo.GetPatientIdByUserId(id); 
            if (patientId == null) throw new Exception("Patient not found");
            var review = dto.ToReview(appointmentId);
            var appointment = await appointmentRepo.AddReviewAsync(review, patientId);
            
            await notificationServ.SendPushNotificationAsync(
                $"New Review",
                $"{appointment.patient?.Name} just left a review: {review.rating/2f} stars",
                $"{review.feedback.Substring(0,Math.Min(50, review.feedback.Length)).TrimEnd()}...",
                [appointment.doctor?.UserId?? ""], NotificationContext.APPOINTMENT_REQUEST);

            
            return Created(review.id, review.ToReadReviewDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to add review");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to add review",
                e.Message
            ));
        }
    }
}