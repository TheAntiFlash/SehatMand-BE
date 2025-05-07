using System.Net.Http.Json;
using System.Text;
using AgoraIO.Media;
using AgoraIO.TokenBuilders;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using SehatMand.Domain.Interface.Service;
using SehatMand.Domain.Utils.Agora;

namespace SehatMand.Infrastructure.Service;

public class AgoraService(IOptions<AgoraSettings> conf, IHttpClientFactory clientFactory): IAgoraService
{
    private readonly string _appId = conf.Value.AppId;
    private readonly string _appCertificate = conf.Value.AppCertificate;
    
    private string AuthToken => Convert.ToBase64String(Encoding.UTF8.GetBytes(conf.Value.ClientKey + ":" + conf.Value.ClientSecret));
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

    public async Task<string> AcquireCloudRecordingId(string channelName, uint recordingId = 3)
    {
        var httpClient = clientFactory.CreateClient("Agora");
        var uri = $"/v1/apps/{_appId}/cloud_recording/acquire";
        var payload = new
        {
            cname = channelName,
            uid = recordingId.ToString(),
            clientRequest = new
            {
                resourceExpiredHour = 24
            }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Authorization", $"Basic {AuthToken}");
        request.Content = JsonContent.Create(payload);
        
        var response = await httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseString);
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        response.EnsureSuccessStatusCode();
        if (!json.TryGetValue("resourceId", out var resourceId))
        {
            throw new Exception("Failed to acquire cloud recording resource ID");
        }
        return (string) resourceId;

    }

    public async Task<string> StartCloudRecording(string channelName, string resourceId, uint recordingId = 3)
    {
        var rtcToken = GenerateRtcToken(recordingId, channelName);
        var httpClient = clientFactory.CreateClient("Agora");
        var uri = $"/v1/apps/{_appId}/cloud_recording/resourceid/{resourceId}/mode/mix/start";
        var payload = new
        {
            cname = channelName,
            uid = recordingId.ToString(),
            clientRequest = new
            {
                Token = rtcToken,
                storageConfig = new
                {
                    vendor = 1,
                    region = 21,
                    bucket = "sehatmand",
                    accessKey = "AKIAWAA66DWDOB4GPIKA",
                    secretKey = "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",
                    fileNamePrefix = (string[]) ["recordings"]
                },
                recordingConfig = new
                {
                    channelType = 0,
                    streamTypes = 0
                }
            }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Authorization", $"Basic {AuthToken}");
        request.Content = JsonContent.Create(payload);
        
        var response = await httpClient.SendAsync(request);
        var responseString = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseString);
        Console.WriteLine(await response.Content.ReadAsStringAsync());

        
        response.EnsureSuccessStatusCode();
        
        return json.TryGetValue("sid", out var sid) ? (string) sid : throw new Exception("Failed to start cloud recording");
    }

    public async Task<(string rid, string sid)> Record(string cname)
    {
        var rId = await AcquireCloudRecordingId(cname);
        var sId = await StartCloudRecording(cname, rId);
        return (rId, sId);
    }


    public async Task<string> StopRecording(string cname, string rid, string sid, uint recordingId = 3)
    {
        var httpClient = clientFactory.CreateClient("Agora");
        var uri = $"/v1/apps/{_appId}/cloud_recording/resourceid/{rid}/sid/{sid}/mode/mix/stop";
        var payload = new
        {
            cname,
            uid = recordingId.ToString(),
            clientRequest = new
            {
                async_stop = false,
            }
        };
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.Headers.Add("Authorization", $"Basic {AuthToken}");
        request.Content = JsonContent.Create(payload);
        
        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine(await response.Content.ReadAsStringAsync());

        Console.WriteLine(responseBody);
        
        response.EnsureSuccessStatusCode();
        
        return responseBody;
    }


    // public async Task ScheduleRoom(string appointmentId, DateTime startTime)
    // {
    //     var httpClient = clientFactory.CreateClient("Agora");
    //     var token = GenerateToken();
    //     var url = $"/ap/edu/apps/{_appId}/v2/rooms/{appointmentId}";
    //     var payload = new
    //     {
    //         roomName = appointmentId,
    //         roomType = 0,
    //         roomProperties = new{
    //             schedule =  new {
    //                 startTime = new DateTimeOffset(startTime).ToUnixTimeMilliseconds(),
    //                 duration = 3600
    //             } 
    //         }
    //     };
    //     using var request = new HttpRequestMessage(HttpMethod.Post, url);
    //     request.Headers.Add("Authorization", $"agora token={token}");
    //     request.Content = JsonContent.Create(payload);
    //     
    //     var response = await httpClient.SendAsync(request);
    //     var responseBody = await response.Content.ReadAsStringAsync();
    //     Console.WriteLine(responseBody);
    //     
    //     response.EnsureSuccessStatusCode();
    //     
    //     
    //
    // }
}