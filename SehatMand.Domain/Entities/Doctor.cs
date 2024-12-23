namespace SehatMand.Domain.Entities;

public partial class Doctor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string? UserId { get; set; } = null!;

    public string Name { get; set; } = null!;
    
    public string? ProfilePictureUrl { get; set; }
    public string? DoctorPaymentId { get; set; }

    public string FatherName { get; set; } = null!;
    public string RegistrationType { get; set; } = null!;
    public DateTime RegistrationDate { get; set; }
    public DateTime LicenseExpiry { get; set; }
    public string SpecialityId { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? ClinicId { get; set; }

    public string? ProfileInfo { get; set; }

    public string ApprovalStatus { get; set; } = null!;
    public string? City { get; set; }

    public string RegistrationId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public virtual ICollection<Appointment> Appointment { get; set; } = new List<Appointment>();

    public virtual ICollection<DoctorDailyAvailability> DoctorDailyAvailability { get; set; } = new List<DoctorDailyAvailability>();

    public virtual ICollection<MedicalForumComment> MedicalForumComment { get; set; } = new List<MedicalForumComment>();

    public virtual Clinic? Clinic { get; set; }

    public virtual User? User { get; set; }  
    
    public virtual Speciality? Speciality { get; set; }

    public List<Qualification> Qualifications { get; set; } = [];
}
