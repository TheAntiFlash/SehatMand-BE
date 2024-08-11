using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SehatMand.Application.Dto.PmcDoctor;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class DoctorVerificationService(
    IHttpClientFactory httpFactory,
    ILogger<DoctorVerificationService> logger
    ): IDoctorVerificationService
{
    public async Task<PmcDoctor> VerifyDoctor(string pmcCode)
    {
        using var client = httpFactory.CreateClient("PmcHttpClient");
        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new
            {
                RegistrationNo = pmcCode
            }),
            Encoding.UTF8,
            "application/json");
        using HttpResponseMessage response = await client.PostAsync("DRC/GetQualifications", jsonContent);
        
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadFromJsonAsync<PmcDoctor>();
        logger.LogInformation($"PMC Verification Response: {jsonResponse}");
        return jsonResponse?? throw new Exception("Unable to verify at this time. Please try again later");
    }
}