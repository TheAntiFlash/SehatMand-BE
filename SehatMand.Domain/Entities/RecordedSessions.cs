namespace SehatMand.Domain.Entities;

public partial class RecordedSessions
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string? appointment_id { get; set; }
    
    public string resource_id { get; set; } = null!;
    
    public string start_id { get; set; } = null!; 

    public string? session_link { get; set; }

    public DateTime? created_at { get; set; } = DateTime.Now;

    public virtual ICollection<Transcriptions> Transcriptions { get; set; } = new List<Transcriptions>();

    public virtual Appointment? appointment { get; set; }
}
