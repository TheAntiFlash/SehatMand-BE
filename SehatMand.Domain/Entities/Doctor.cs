namespace SehatMand.Domain.Entities;

public partial class Doctor
{
    public string id { get; set; } = null!;

    public string userid { get; set; } = null!;

    public string name { get; set; } = null!;

    public string specialty { get; set; } = null!;

    public string phone { get; set; } = null!;

    public string email { get; set; } = null!;

    public string? address { get; set; }

    public string? clinic_id { get; set; }

    public string? profile_info { get; set; }

    public string approval_status { get; set; } = null!;

    public string registration_id { get; set; } = null!;

    public DateTime created_at { get; set; }

    public DateTime? modified_at { get; set; }

    public virtual ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorDailyAvailability> DoctorDailyAvailability { get; set; } = new List<DoctorDailyAvailability>();

    public virtual ICollection<MedicalForumComment> MedicalForumComment { get; set; } = new List<MedicalForumComment>();

    public virtual Clinic? clinic { get; set; }

    public virtual User user { get; set; } = null!;
}
