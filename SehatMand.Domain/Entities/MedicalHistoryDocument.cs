namespace SehatMand.Domain.Entities;

public partial class MedicalHistoryDocument
{
    public string id { get; set; } = Guid.NewGuid().ToString();

    public string? appointment_id { get; set; }

    public string? symptoms { get; set; }

    public string? diagnosed_disease { get; set; }

    public string patient_id { get; set; } = null!;

    public string? document_path { get; set; }
    public string? document_name { get; set; }
    public string? document_description { get; set; }

    public string? doctors_comments { get; set; }

    public DateTime record_date { get; set; }

    public DateTime created_at { get; set; }

    public string created_by { get; set; } = null!;

    public DateTime? modified_at { get; set; }

    public string? modified_by { get; set; }

    public virtual Appointment? appointment { get; set; }

    public virtual User created_byNavigation { get; set; } = null!;

    public virtual Patient patient { get; set; } = null!;

    public virtual User? modified_byNavigation { get; set; }
}
