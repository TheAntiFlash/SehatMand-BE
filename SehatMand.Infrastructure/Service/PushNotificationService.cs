using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SehatMand.Domain.Interface.Service;

namespace SehatMand.Infrastructure.Service;

public class PushNotificationService(
    IHttpClientFactory httpClientFactory,
    ILogger<PushNotificationService> logger,
    IConfiguration config
    ): IPushNotificationService
{
    public async Task SendPushNotificationAsync(string title, string subtitle, string message, /*string? imageUrl,*/
        List<string> users, string context, string? screenPath = null)
    {
        //imageUrl = config.GetSection("Wso2:ApiManager:BaseUrl").Value +"/assets/1"+ imageUrl;
        var httpClient = httpClientFactory.CreateClient("OneSignal");
        const string url = "/notifications";
        var appId = config.GetSection("OneSignal:AppId").Value ?? throw new Exception("OneSignal App ID not found in environment");
        var payload = new
        {
            app_id = appId,
            name = context,
            headings = new { en = title },
            subtitle = new { en = subtitle },
            contents = new { en = message },
            target_channel = "push",
            /*big_picture = imageUrl,
            ios_attachments = imageUrl,*/
            include_aliases = new
            {
                external_id = users.ToArray()
            }
        };
        
        
    
        logger.LogInformation(payload.ToString());
        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = JsonContent.Create(payload);
    
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    
        var responseBody = await response.Content.ReadAsStringAsync();
        logger.LogInformation($"Notification sent successfully: {responseBody}"); 
    }
}