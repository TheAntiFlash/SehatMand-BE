namespace SehatMand.Domain.Entities;

public partial class Appointment
{
    public string id { get; set; } = null!;

    public string patient_id { get; set; } = null!;

    public string doctor_id { get; set; } = null!;

    public bool online { get; set; }

    public decimal latitude { get; set; }

    public decimal longitude { get; set; }

    public string status { get; set; } = null!;

    public DateTime appointment_date { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public virtual ICollection<Billing> Billing { get; set; } = new List<Billing>();

    public virtual ICollection<RecordedSessions> RecordedSessions { get; set; } = new List<RecordedSessions>();

    public virtual ICollection<Review> Review { get; set; } = new List<Review>();

    public virtual Doctor doctor { get; set; } = null!;

    public virtual Patient patient { get; set; } = null!;
}
