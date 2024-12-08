namespace SehatMand.Domain.Entities;

public partial class Review
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string appointment_id { get; set; }

    public int rating { get; set; }

    public string feedback { get; set; }

    public DateTime created_at { get; set; } = DateTime.Now;

    public virtual Appointment? appointment { get; set; }
}
