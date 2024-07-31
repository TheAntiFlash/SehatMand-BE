
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Application.Mapper;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;
using Zong_HRM.Application.DTOs.Error;

namespace SehatMand.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientController(IAuthRepository repo) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDto creds)
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
            return StatusCode(500, new ErrorResponseDto(
                "Unable to delete employee in geofence",
                e.Message
            ));
        }
       

    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterPatientDto dto)
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
            return StatusCode(500, new ErrorResponseDto(
                "Unable to register patient",
                e.Message
            ));
        }
    }
    [HttpPut]
    [Route("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
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
            return StatusCode(500, new ErrorResponseDto(
                "Unable to reset password",
                e.Message
            ));
        }
    }
}   

