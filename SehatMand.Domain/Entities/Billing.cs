namespace SehatMand.Domain.Entities;

public partial class Billing
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string appointment_id { get; set; } = null!;

    public decimal amount { get; set; }

    public string status { get; set; } = null!;

    public DateTime transaction_date { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public virtual Appointment appointment { get; set; } = null!;
}
