namespace SehatMand.Domain.Interface.Service;

public interface IAudioRebuilderService
{
    public Task<string> ProcessAudioAsync(string sid, string appointmentId);
}