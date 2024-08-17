using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IPatientRepository
{
    public Task<Patient?> GetByEmailAsync(string email);

    public Task<bool> CompletePatientProfile(
        string id,
        string address,
        string city,
        float height,
        float weight,
        string gender,
        string bloodGroup,
        DateTime dateOfBirth
    );

    public Task<bool> UpdatePatientProfile(
        string id,
        string? fullName,
        string email,
        string phoneNumber,
        string address,
        string city,
        float height,
        float weight,
        string gender,
        string bloodGroup,
        DateTime? dateOfBirth,
        string profileInfo
    );

    public Task<Patient?> GetByIdAsync(string id);
}