
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SehatMand.Application.Dto.Authentication;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DoctorController(IAuthRepository repo) : ControllerBase
{
    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDto creds)
    {
        var response = await repo.LoginDoctor(creds.Email, creds.Password);

        if (response == null)
        {
            return Unauthorized("Invalid credentials");
        }

        return Ok(response);

    }

}   

