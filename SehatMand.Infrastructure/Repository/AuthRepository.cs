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
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class AuthRepository(
    IConfiguration conf,
    SmDbContext dbContext,
    IDoctorRepository docRepo,
    IPatientRepository patientRepo
    ): IAuthRepository
{
    public Task<string?> RegisterDoctor(Doctor doctor)
    {
        User user = doctor.User;
        user.PasswordHash
            = BCrypt.Net.BCrypt.HashPassword(doctor.User.PasswordHash);
        throw new NotImplementedException();
        
    }

    //consider fixing so a doctor can make a patient accound and a patient can make a doctor account
    public async Task<string?> RegisterPatient(Patient patient)
    {
        var exists = await dbContext.User.AnyAsync(u => u.Email == patient.Email);
        if (exists)
        {
            return null;
        }
        await dbContext.User.AddAsync(patient.User);
        await dbContext.Patient.AddAsync(patient);
        await dbContext.SaveChangesAsync();
        return CreateToken(patient.User);
    }

    public async Task<string?> LoginPatient(string email, string password)
    {
        var patient = await patientRepo.GetByEmailAsync(email);
        if (patient == null)
        {
            return null;
        }

        return 
            BCrypt.Net.BCrypt.Verify(password, patient.User.PasswordHash) ?
            CreateToken(patient.User) :
            null;
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

    public async Task<bool> ForgotPassword(string dtoEmail, string dtoNewPassword, string dtoPhoneNumber)
    {
        var patient = await dbContext.Patient.Include(p => p.User).FirstOrDefaultAsync(p => p.Email == dtoEmail && p.Phone == dtoPhoneNumber);
        if (patient == null) return false;
        
        patient.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dtoNewPassword);
        await dbContext.SaveChangesAsync();
        return true;
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
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