
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Appointment;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Patient;
using SehatMand.Application.Mapper;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;
using Zong_HRM.Application.DTOs.Error;

namespace SehatMand.API.Controllers;

[Route("api/patient")]
[ApiController]
public class PatientController(
    IAuthRepository repo,
    IPatientRepository patientRepo,
    ILogger<PatientController> logger
    ) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto creds)
    {
        try
        {
            var response = await repo.LoginPatient(creds.Email, creds.Password);

            if (response == null)
            {
                return Unauthorized(new ErrorResponseDto(
                    "Invalid Credentials",
                    "Email or password is incorrect"
                    ));
            }

            return Ok(response);
        } catch (Exception e)
        {
            logger.LogError(e, "Unable to login");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to login",
                e.Message
            ));
        }
       

    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody]RegisterPatientDto dto)
    {
        try
        {
            dto.ValidatePassword();
            var response = await repo.RegisterPatient(dto.ToPatient());

            if (response == null)
            {
                return BadRequest(new ErrorResponseDto(
                    "Email already exists",
                    "A user with this email already exists"
                ));
            }

            return Ok(response);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to register patient");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to register patient",
                e.Message
            ));
        }
    }
    [HttpPut]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
        try
        {
            if (dto.NewPassword != dto.ConfirmPassword) throw new Exception("Passwords do not match");
            
            var response = await repo.ForgotPassword(dto.Email, dto.NewPassword, dto.PhoneNumber);

            if (!response)
            {
                return NotFound(new ErrorResponseDto(
                    "Invalid Details",
                    "provided details are invalid"
                ));
            }

            var result = "Password successfully reset";
            return Ok(new {result});
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to reset password");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to reset password",
                e.Message
            ));
        }
    }
    
    [Authorize]
    [HttpPatch]
    [Route("update-password")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ErrorResponseDto("Error", "Something went wrong"));
        
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
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to update password",
                e.Message
            ));
        }
    }

    [Authorize]
    [HttpPatch]
    [Route("complete-profile")]
    public async Task<IActionResult> CompleteProfile([FromBody] CompletePatientProfileDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ErrorResponseDto("Error", "Something went wrong"));
            
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

            return BadRequest(new ErrorResponseDto("Error", "Something went wrong"));
        } catch (Exception e)
        {
            logger.LogError(e, "Unable to complete profile");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to complete profile",
                e.Message
            ));
        }
    }
    [Authorize]
    [HttpPatch]
    [Route("update-profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePatientProfileDto dto)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ErrorResponseDto("Error", "Something went wrong"));
        
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
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to update profile",
                e.Message
            ));
        }
    }
    
    [Authorize]
    [HttpGet]
    [Route("profile")]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ErrorResponseDto("Error", "Something went wrong"));
        
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
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to get profile",
                e.Message
            ));
        }
    }

    
}   

