namespace SehatMand.Domain.Entities;

public partial class RecordedSessions
{
    public string id { get; set; } = null!;

    public string? appointment_id { get; set; }

    public string? session_link { get; set; }

    public DateTime? created_at { get; set; }

    public virtual ICollection<Transcriptions> Transcriptions { get; set; } = new List<Transcriptions>();

    public virtual Appointment? appointment { get; set; }
}
