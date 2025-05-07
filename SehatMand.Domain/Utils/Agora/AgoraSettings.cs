namespace SehatMand.Domain.Utils.Agora;

public record AgoraSettings
{
    public string AppId { get; set; }
    public string AppCertificate { get; set; }
    
    public string ClientKey { get; set; }
    public string ClientSecret { get; set; }
}