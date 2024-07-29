using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using SehatMand.Domain;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;

namespace SehatMand.Infrastructure.Repository;

public class AuthRepository(
    IConfiguration conf,
    DbContext dbContext,
    IDoctorRepository docRepo
    ): IAuthRepository
{
    public Task<string?> RegisterDoctor(Doctor doctor)
    {
        User user = doctor.User;
        user.PasswordHash
            = BCrypt.Net.BCrypt.HashPassword(doctor.User.PasswordHash);
        throw new NotImplementedException();
        
    }

    public Task<string?> RegisterPatient(Patient patient)
    {
        throw new NotImplementedException();
    }

    public Task<string?> LoginPatient(string email, string password)
    {
        throw new NotImplementedException();
    }

    public async Task<string?> LoginDoctor(string email, string password)
    {
        var doctor = await docRepo.GetByEmailAsync(email);
        if (doctor == null)
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(password, doctor.User.PasswordHash)) 
        {
            return null;
        }

        return CreateToken(doctor.User);
    }
    
    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new (ClaimTypes.Name, user.Username),
            new (ClaimTypes.Email, user.Email),
            new (ClaimTypes.Role, user.Role)
        };
        
        var key = 
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    conf.GetSection("JWT:Key").Value!
                )
            );
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
        var token = new JwtSecurityToken(
            issuer: "sehatmand.pk",
            claims: claims,
            expires: DateTime.Now.AddHours(8),
            signingCredentials: creds
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

}