namespace SehatMand.Domain.Entities;

public partial class DoctorDailyAvailability
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string? doctor_id { get; set; }

    public int? day_of_week { get; set; }

    public DateTime? availability_start { get; set; }

    public DateTime? availability_end { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public string? created_by { get; set; }

    public virtual User? created_byNavigation { get; set; }

    public virtual Doctor? doctor { get; set; }
}
