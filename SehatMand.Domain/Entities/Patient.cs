namespace SehatMand.Domain.Entities;

public partial class Patient
{
    public string id { get; set; } = null!;

    public string userid { get; set; } = null!;

    public string name { get; set; } = null!;

    public DateTime date_of_birth { get; set; }

    public string phone { get; set; } = null!;

    public string email { get; set; } = null!;

    public string? address { get; set; }

    public string? profile_info { get; set; }

    public int? weight { get; set; }

    public string? blood_group { get; set; }

    public int? height { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public virtual ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalForumPost> MedicalForumPost { get; set; } = new List<MedicalForumPost>();

    public virtual ICollection<MedicalHistory> MedicalHistory { get; set; } = new List<MedicalHistory>();

    public virtual User user { get; set; } = null!;
}
