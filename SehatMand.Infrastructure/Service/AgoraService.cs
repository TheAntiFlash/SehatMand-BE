using System.Net.Http.Json;
using AgoraIO.Media;
using AgoraIO.Rtm;
using Microsoft.Extensions.Options;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Agora;

namespace SehatMand.Infrastructure.Service;

public class AgoraService(IOptions<AgoraSettings> conf, IHttpClientFactory clientFactory): IAgoraService
{
    private readonly string _appId = conf.Value.AppId;
    private readonly string _appCertificate = conf.Value.AppCertificate;
    private string GenerateToken()
    {
        var accessToken = new AccessToken2(_appId, _appCertificate, 36000);
        return accessToken.build();
    }

    public string GenerateRtcToken(uint uid, string channel)
    {
        var token = RtcTokenBuilder2
            .buildTokenWithUid(
                _appId,
                _appCertificate,
                channel,
                uid,
                RtcTokenBuilder2.Role.RolePublisher,
                3600,
                3600
                );
        return token;

    }

    public async Task ScheduleRoom(string appointmentId, DateTime startTime)
    {
        var httpClient = clientFactory.CreateClient("Agora");
        var token = GenerateToken();
        var url = $"/ap/edu/apps/{_appId}/v2/rooms/{appointmentId}";
        var payload = new
        {
            roomName = appointmentId,
            roomType = 0,
            roomProperties = new{
                schedule =  new {
                    startTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds(),
                    duration = 3600
                } 
            }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"agora token={token}");
        request.Content = JsonContent.Create(payload);
        
        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseBody);
        
        response.EnsureSuccessStatusCode();
        
        

    }
}