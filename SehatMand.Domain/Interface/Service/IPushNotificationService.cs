namespace SehatMand.Domain.Interface.Service;

public interface IPushNotificationService
{
    public Task SendPushNotificationAsync(string title, string subtitle, string message, string? imageUrl,
        List<string> users, string context);
}