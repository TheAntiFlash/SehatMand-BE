namespace SehatMand.Domain.Entities;

public partial class Clinic
{
    public string id { get; set; } = null!;

    public string? name { get; set; }

    public string? city { get; set; }

    public decimal? latitude { get; set; }

    public decimal? longitude { get; set; }

    public DateTime? created_at { get; set; }

    public string? created_by { get; set; }

    public DateTime? modified_at { get; set; }

    public string? modified_by { get; set; }

    public virtual ICollection<Doctor> Doctor { get; set; } = new List<Doctor>();

    public virtual User? created_byNavigation { get; set; }

    public virtual User? modified_byNavigation { get; set; }
}
