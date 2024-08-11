using SehatMand.Application.Dto.PmcDoctor;

namespace SehatMand.Domain.Interface.Service;

public interface IDoctorVerificationService
{
    Task<PmcDoctor> VerifyDoctor(string pmcCode);
}