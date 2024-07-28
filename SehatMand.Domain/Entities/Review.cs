namespace SehatMand.Domain.Entities;

public partial class Review
{
    public string id { get; set; } = null!;

    public string? appointment_id { get; set; }

    public int? rating { get; set; }

    public string? feedback { get; set; }

    public DateTime? created_at { get; set; }

    public virtual Appointment? appointment { get; set; }
}
