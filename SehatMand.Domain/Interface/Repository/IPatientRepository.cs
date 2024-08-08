using SehatMand.Domain.Entities;

namespace SehatMand.Domain.Interface.Repository;

public interface IPatientRepository
{
    public Task<Patient?> GetByEmailAsync(string email);

    public Task<bool> UpdatePatientProfile(
        string id,
        string address,
        string city,
        float height,
        float weight,
        string gender,
        string bloodGroup,
        DateTime dateOfBirth
    );
}