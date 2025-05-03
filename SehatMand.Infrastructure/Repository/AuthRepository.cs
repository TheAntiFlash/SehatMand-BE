using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;
using Exception = System.Exception;

namespace SehatMand.Infrastructure.Repository;

public class AuthRepository(
    IConfiguration conf,
    SmDbContext dbContext,
    IDoctorRepository docRepo,
    IUserRepository userRepo,
    IPaymentService stripeServ,
    IStorageService storageServ,
    IPatientRepository patientRepo
    ): IAuthRepository
{
    public async Task<string?> RegisterDoctor(Doctor? doctor, IFormFile dtoProfilePicture)
    {
        
        var exists = await dbContext.User.FirstOrDefaultAsync(u => u.Email == doctor.Email);
        
        if (exists != null)
        {
            if (!exists.IsActive)
            {
               throw new Exception("Doctor account already exists. Verify email to activate"); 
            }

            return null;
        }
        // payment
        var docPaymentId = await stripeServ.CreateDoctorAccountAsync(doctor.Email,doctor.Name, doctor.FatherName);
        doctor.DoctorPaymentId = docPaymentId;
        
        //profile picture
        var basePath = Path.Join("doctor","profile-picture");
        await storageServ.UploadFileAsync(dtoProfilePicture, doctor.Id, basePath); // upload
        doctor.ProfilePictureUrl = Path.Join(basePath, doctor.Id + Path.GetExtension(dtoProfilePicture.FileName)); // save path to db
        await dbContext.User.AddAsync(doctor.User);
        await dbContext.Doctor.AddAsync(doctor);
        await dbContext.SaveChangesAsync();
        return "";  //CreateToken(doctor.User);

    }

    //consider fixing so a doctor can make a patient accound and a patient can make a doctor account
    public async Task<string?> RegisterPatient(Patient? patient)
    {
        var exists = await dbContext.User.FirstOrDefaultAsync(u => u.Email == patient.Email);
        
        if (exists != null)
        {
            if (!exists.IsActive)
            {
               throw new Exception("Patient account already exists. Verify email to activate"); 
            }

            return null;
        }
        await dbContext.User.AddAsync(patient.User);
        await dbContext.Patient.AddAsync(patient);
        await dbContext.SaveChangesAsync();
        return "";  //CreateToken(patient.User);
    }

    public async Task<string?> LoginPatient(string email, string password)
    {
        var patient = await patientRepo.GetByEmailAsync(email);
        if (patient == null)
        {
            return null;
        }

        if (!patient.User.IsActive)
        {
            throw new Exception("Account not activated. Please verify your email. Or Contact Admin");
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
        if (!doctor.User.IsActive)
        {
            throw new Exception("Account not active. Please verify your email. Or Contact Admin");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, doctor.User.PasswordHash)) 
        {
            return null;
        }

        return CreateToken(doctor.User);
    }

    public async Task<bool> ForgotPassword(string dtoEmail, string dtoNewPassword, string otp)
    {
        var user = await dbContext.User.FirstOrDefaultAsync(p => p.Email == dtoEmail);
        if (user == null) return false;
        if (user.Otp != otp) throw new Exception("Invalid OTP");
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dtoNewPassword);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<string?> LoginAdmin(string email, string password)
    {
        var admin = await userRepo.GetByEmailAsync(email);
        if (admin == null)
        {
            return null;
        }
        if (!admin.IsActive)
        {
            throw new Exception("Account not active. Please verify your email. Or Contact Admin");
        }
        if (admin.Role != "Admin")
        {
            throw new Exception("Not an admin account");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, admin.PasswordHash)) 
        {
            return null;
        }

        return CreateToken(admin); 
    }

    private string CreateToken(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        ];
        
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
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds
        );
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return jwt;
    }

}