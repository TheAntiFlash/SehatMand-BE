
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Authentication;
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
public class DoctorController(IAuthRepository repo, IDoctorVerificationService service) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto creds)
    {
        var response = await repo.LoginDoctor(creds.Email, creds.Password);

        if (response == null)
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(response);

    }
    
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDoctorDto dto)
    {
        try
        {
            dto.ValidatePassword();
            var pmcDoctorInfo = await service.VerifyDoctor(dto.PmcRegistrationCode);
            var response = await repo.RegisterDoctor(dto.ToDoctor(pmcDoctorInfo));
            if(response == null)
            {
                return BadRequest("Email already exists");
            }
            return Ok(response);
    
        }
        catch (Exception e)
        {
            return StatusCode(500, new ErrorResponseDto(
                "Unable to register",
                e.Message
            ));
        }
    }

}   

