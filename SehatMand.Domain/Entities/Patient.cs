namespace SehatMand.Domain.Entities;

public partial class Patient
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string UserId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? ProfileInfo { get; set; }

    public int? Weight { get; set; }

    public string? BloodGroup { get; set; }

    public int? Height { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();

    public virtual ICollection<MedicalForumPost> MedicalForumPost { get; set; } = new List<MedicalForumPost>();

    public virtual ICollection<MedicalHistory> MedicalHistory { get; set; } = new List<MedicalHistory>();

    public virtual User User { get; set; } = null!;
}
