using Microsoft.AspNetCore.Http;
using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IAuthRepository
{
    Task<string?> RegisterDoctor(Doctor? doctor, IFormFile dtoProfilePicture);
    Task<string?> RegisterPatient(Patient? patient);
    Task<string?> LoginPatient(string email, string password);
    Task<string?> LoginDoctor(string email, string password);
    Task<bool> ForgotPassword(string dtoEmail, string dtoNewPassword, string dtoPhoneNumber);
    Task<string?> LoginAdmin(string credsEmail, string credsPassword);
}