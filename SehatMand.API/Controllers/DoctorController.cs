
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Dto.Doctor;
using SehatMand.Application.Mapper;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;
using Zong_HRM.Application.DTOs.Error;

namespace SehatMand.API.Controllers;

[Route("api/doctor")]
[ApiController]
public class DoctorController(
    IAuthRepository repo,
    IPatientRepository patientRepo, 
    IDoctorRepository docRepo, 
    IDoctorVerificationService service,
    ILogger<DoctorController> logger) : ControllerBase
{
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
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to login",
                e.Message
            ));
        }

    }
    
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
            
            var response = await repo.RegisterDoctor(dto.ToDoctor(pmcDoctorInfo));
            if(response == null)
            {
                return BadRequest("Email already exists");
            }
            return Ok(response);
    
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to register");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to register",
                e.Message
            ));
        }
    }
    
    

    [Authorize]
    [HttpGet]
    [Route("nearest-doctors")]
    public async Task<IActionResult> GetNearestDoctors()
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
            var doctors = await docRepo.GetNearestDoctors(patient.City);
            return Ok(doctors.Select(d => d.ToReadNearestDoctorDto()).ToList());
        }
        catch (Exception e)
        {
            logger.LogError(e, "Unable to get nearest doctors");
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to get nearest doctors",
                e.Message
            ));
        }
    }

    [Authorize]
    [HttpPatch]
    [Route("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateDoctorProfileDto dto)
    {
        throw new NotImplementedException();
    }
    
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
            return StatusCode(StatusCodes.Status400BadRequest, new ErrorResponseDto(
                "Unable to get profile",
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
            
            var doctor = await docRepo.GetByUserIdAsync(id);
            if (doctor == null) throw new Exception("Doctor not found");
            return Ok(doctor.ToReadDoctorProfileDto());
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
            if (id == null) throw new Exception("User not found");
            await docRepo.UpdatePassword(id, dto.OldPassword, dto.NewPassword);
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
    
    
}   

