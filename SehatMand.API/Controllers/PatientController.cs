
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Dto.Patient;
using SehatMand.Application.Mapper;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;
using SehatMand.Infrastructure.Service;

namespace SehatMand.API.Controllers;

/// <summary>
/// This controller is responsible for handling all the patient related requests.
/// It includes the following functionalities:
/// * - Login
/// * - Register
/// * - Forgot Password
/// * - Update Password
/// * - Complete Profile
/// * - Update Profile
/// * - Get Profile
/// </summary>
/// <param name="repo"></param>
/// <param name="patientRepo"></param>
/// <param name="smtp"></param>
/// <param name="otpServ"></param>
/// <param name="logger"></param>
[Route("api/patient")]
[ApiController]
public class PatientController(
    IAuthRepository repo,
    IPatientRepository patientRepo,
    IEmailService smtp,
    IOtpService otpServ,
    ILogger<PatientController> logger
    ) : ControllerBase
{
    /// <summary>
    /// Login patient with email and password
    /// </summary>
    /// <param name="creds"></param>
    /// <returns>Returns a JWT Token</returns>
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto creds)
    {
        try
        {
            var response = await repo.LoginPatient(creds.Email, creds.Password);

            if (response == null)
            {
                return Unauthorized(new ResponseDto(
                    "Invalid Credentials",
                    "Email or password is incorrect"
                    ));
            }

            return Ok(response);
        } catch (Exception e)
        {
            logger.LogError(e, "Unable to login");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to login",
                e.Message
            ));
        }
       

    }
    
    /// <summary>
    /// Register a patient
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody]RegisterPatientDto dto)
    {
        try
        {
            dto.ValidatePassword();
            var patient = dto.ToPatient();
            var response = await repo.RegisterPatient(patient);

            if (response == null)
            {
                return BadRequest(new ResponseDto(
                    "Email already exists",
                    "A user with this email already exists"
                ));
            }
            var otp = OtpService.GenerateOtp();
            await otpServ.SaveOtpForUserAsync(
                patient?.User.Id?? throw new Exception("Something went wrong while registering patient"),
                otp, 10);
            await smtp.SendOtpEmailAsync(patient.Email, otp, patient.Name, 10);
            
            return Ok(new {userId = patient.User.Id});
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to register patient");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to register patient",
                e.Message
            ));
        }
    }
    
    /// <summary>
    /// Update password
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
            Console.WriteLine(id);
            if (id == null) throw new Exception("User not found");
            Console.WriteLine("Old Password: " + dto.OldPassword + " New Password " + dto.NewPassword);
            await patientRepo.UpdatePatientPassword(id, dto.OldPassword, dto.NewPassword);
            
            Console.WriteLine("Password Updated");
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
    /// Complete profile
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPatch]
    [Route("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompletePatientProfileDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
            
            IEnumerable<Claim> claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            if (id == null) throw new Exception("User not found");
            if (email == null) throw new Exception("User not found");
            logger.LogInformation(id);
            logger.LogInformation(email);
            var response = await patientRepo.CompletePatientProfile(
                id,
                dto.Address,
                dto.City,
                dto.HeightInInches,
                dto.Weight,
                dto.Gender,
                dto.BloodGroup,
                dto.DateOfBirth.ToDateTime()
            );
            if (response)
            {
                return Ok();
            }

            return BadRequest(new ResponseDto("Error", "Something went wrong"));
        } catch (Exception e)
        {
            logger.LogError(e, "Unable to complete profile");
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
                "Unable to complete profile",
                e.Message
            ));
        }
    }
    /// <summary>
    /// Update profile
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPatch]
    [Route("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePatientProfileDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;

            await patientRepo.UpdatePatientProfile(
                id,
                dto.FullName,
                dto.Email,
                dto.PhoneNumber,
                dto.Address,
                dto.City,
                dto.Height ?? 0.0f,
                dto.Weight ?? 0.0f,
                dto.Gender,
                dto.BloodGroup,
                dto.DateOfBirth?.ToDateTime() ,  
                dto.ProfileInfo
            );
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
    /// Get profile
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
            var patient = await patientRepo.GetByIdAsync(id);
            if (patient == null) throw new Exception("Patient not found");
            return Ok(patient.ToReadPatientProfileDto());
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
}   

