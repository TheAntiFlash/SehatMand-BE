using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Admin;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController(
    IDoctorRepository doctorRepo,
    IAuthRepository authRepo,
    IAppointmentRepository appointmentRepo,
    IMedicalForumRepository forumRepo,
    IPatientRepository patientRepo,
    ILogger<AdminController> logger
    ): ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto creds)
    {
        try
        {
            var response = await authRepo.LoginAdmin(creds.Email, creds.Password);
            if (response == null)
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to login");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to login",
                e.Message
            ));
        }
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var totalDoctors = await doctorRepo.GetTotalDoctorsAsync();
            var totalPatients = await patientRepo.GetTotalPatientsAsync();
            var totalAppointments = await appointmentRepo.GetTotalAppointmentsAsync();
            var totalForumPosts = await forumRepo.GetTotalForumPostsAsync();
            var appointmentsByMonth = await appointmentRepo.GetAppointmentsByMonthAsync();

            return Ok(new ReadDashboardDataDto(
                totalDoctors,
                totalPatients,
                totalAppointments,
                totalForumPosts,
                appointmentsByMonth.ToReadAppointmentCountByMonthDto()
            ));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get dashboard data");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get dashboard data",
                e.Message
            ));
        }

    }
    
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorsAsync([RegularExpression("rating|appointments|name|joiningDate")]string? orderBy, [RegularExpression("asc|desc")]string? orderDirection)
    {
        try
        {
            var doctors = await doctorRepo.GetForAdminAsync(orderBy, orderDirection);
            return Ok(doctors.Select(d => d.ToDoctorForDashboardDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get doctors");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get doctors",
                e.Message
            ));
        }
    }
    
    [HttpGet("doctor/{doctorId}")]
    public async Task<IActionResult> GetDoctorByIdAsync(string doctorId)
    {
        try
        {
            var doctor = await doctorRepo.GetByIdAsync(doctorId);
            return Ok(doctor.ToReadDoctorProfileForAdminDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get doctor");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get doctor",
                e.Message
            ));
        }
    }
    
    [HttpPatch("doctor/{doctorId}/toggle-status")]
    public async Task<IActionResult> ToggleDoctorStatus(string doctorId)
    {
        try
        {
            await doctorRepo.ToggleActiveStatus(doctorId);
            return Ok();

        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to toggle doctor status");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to toggle doctor status",
                e.Message
            ));
        }
    }
    
    [HttpGet("forum")]
    public async Task<IActionResult> GetForumPosts()
    {
        try
        {
            var posts = await forumRepo.GetRecentPosts(5);
            return Ok(posts.Select(p => p.ToReadForumPostAdminDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get forum posts");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get forum posts",
                e.Message
            ));
        }
    }
    
    [Authorize]
    [HttpGet("appointment")]
    public async Task<IActionResult> GetAppointments([FromQuery] QueryAppointmentStatus? query)
    {
        try
        {
            var appointments = await appointmentRepo.GetAdminAppointmentsAsync(query?.Status);

            return Ok(appointments.Select(a => a.ToReadAppointmentForAdminDto()));
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
}