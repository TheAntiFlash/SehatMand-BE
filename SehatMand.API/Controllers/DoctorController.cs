
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Doctor;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Mapper;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;
using SehatMand.Infrastructure.Service;

namespace SehatMand.API.Controllers;

/// <summary>
/// * This controller is responsible for handling all the doctor related requests.
/// * It includes the following functionalities:
/// * - Login
/// * - Register
/// * - Get nearest doctors
/// * - Update profile
/// * - Get profile by id
/// * - Get profile
/// * - Update password 
/// </summary>
/// <param name="repo"></param>
/// <param name="patientRepo"></param>
/// <param name="docRepo"></param>
/// <param name="service"></param>
/// <param name="smtp"></param>
/// <param name="otpServ"></param>
/// <param name="logger"></param>
[Route("api/doctor")]
[ApiController]
public class DoctorController(
    IAuthRepository repo,
    IPatientRepository patientRepo, 
    IDoctorRepository docRepo, 
    IDoctorVerificationService service,
    IEmailService smtp,
    IOtpService otpServ,
    ILogger<DoctorController> logger) : ControllerBase
{
    /// <summary>
    /// Login doctor with email and password
    /// </summary>
    /// <param name="creds"></param>
    /// <returns>Returns a JWT Token</returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto creds)
    {
        try
        {
            var response = await repo.LoginDoctor(creds.Email, creds.Password);
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
    
    /// <summary>
    /// Register a doctor
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDoctorDto dto)
    {
        try
        {
            dto.ValidatePassword();
            var pmcDoctorInfo = await service.VerifyDoctor(dto.PmcRegistrationCode);
            Console.WriteLine(pmcDoctorInfo);
            if (pmcDoctorInfo == null)
            {
                return BadRequest("Invalid PMC registration code");
            }

            var doctor = dto.ToDoctor(pmcDoctorInfo);
            var response = await repo.RegisterDoctor(doctor);
            if(response == null)
            {
                return BadRequest("Email already exists");
            }

            var otp = OtpService.GenerateOtp();
            await otpServ.SaveOtpForUserAsync(
                doctor?.User.Id?? throw new Exception("Something went wrong while registering doctor"),
                otp, 5);
            await smtp.SendOtpEmailAsync(doctor.Email, otp, doctor.Name, 10);
            
            return Ok(new {userId = doctor.User.Email});
    
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to register");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to register",
                e.Message
            ));
        }
    }
    
    
    /// <summary>
    /// Get doctors queryable by name and speciality
    /// </summary>
    /// <param name="name"></param>
    /// <param name="speciality"></param>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] string? name, [FromQuery] string? speciality)
    {
        try
        {
            var doctors = await docRepo.GetAsync(name, speciality);
            return Ok(doctors.Select(d => d.ToReadNearestDoctorDto()).ToList());
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
    
    /// <summary>
    /// Get nearest doctors
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpGet]
    [Route("nearest-doctors")]
    public async Task<IActionResult> GetNearestDoctors()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patient = await patientRepo.GetByIdAsync(id);
            if (patient == null) throw new Exception("Patient not found");
            var doctors = await docRepo.GetNearestDoctors(patient.City);
            return Ok(doctors.Select(d => d.ToReadNearestDoctorDto()).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get nearest doctors");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get nearest doctors",
                e.Message
            ));
        }
    }

    /// <summary>
    /// Update doctor profile
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPatch]
    [Route("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDoctorProfileDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            
            await docRepo.UpdateProfile(id, dto.City, dto.Address, dto.ProfileInfo,
                dto.Speciality, dto.Availabilities?.Select(d => d.ToDoctorAvailability()), dto.Phone, dto.ClinicId);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update profile");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to update profile",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Get doctor profile by id
    /// </summary>
    /// <param name="Id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet]
    [Route("profile/{Id}")]
    public async Task<IActionResult> GetProfileById([FromRoute] string Id)
    {
        try
        {
            
            var doctor = await docRepo.GetByIdAsync(Id);
            if (doctor == null) throw new Exception("Doctor not found");
            return Ok(doctor.ToReadDoctorProfileDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get profile");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get profile",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Get doctor profile
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpGet]
    [Route("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            
            var doctor = await docRepo.GetByUserIdAsync(id);
            if (doctor == null) throw new Exception("Doctor not found");
            return Ok(doctor.ToReadDoctorProfileDto());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get profile");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get profile",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Update doctor password
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPatch]
    [Route("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            await docRepo.UpdatePassword(id, dto.OldPassword, dto.NewPassword);
            return Ok();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to update password");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to update password",
                e.Message
            ));
        }
    }
    
    
    /// <summary>
    /// Get all specialities
    /// </summary>
    /// <returns></returns>
    [Authorize]
    [HttpGet]
    [Route("speciality")]
    public async Task<IActionResult> GetAllSpecialities()
    {
        try
        {
            var specialities = await docRepo.GetSpecialities();
            return Ok(specialities.Select(s => s.ToReadSpecialityDto()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get specialities");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to get specialities",
                e.Message
            ));
        }
    }
    
}   

