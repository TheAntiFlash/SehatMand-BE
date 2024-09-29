
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
                return NotFound(new ResponseDto(
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
            return StatusCode(StatusCodes.Status400BadRequest, new ResponseDto(
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

