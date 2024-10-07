namespace SehatMand.Domain.Entities;

public partial class User
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public bool IsActive { get; set; }
    
    public string? Otp { get; set; }
    
    public DateTime? OtpExpiry { get; set; }

    public virtual ICollection<Clinic> CliniccreatedByNavigation { get; set; } = new List<Clinic>();

    public virtual ICollection<Clinic> ClinicmodifiedByNavigation { get; set; } = new List<Clinic>();

    public virtual ICollection<Coupon> CouponcreatedByNavigation { get; set; } = new List<Coupon>();

    public virtual ICollection<Coupon> CouponusedByNavigation { get; set; } = new List<Coupon>();

    public virtual ICollection<Doctor> Doctor { get; set; } = new List<Doctor>();

    public virtual ICollection<DoctorDailyAvailability> DoctorDailyAvailability { get; set; } = new List<DoctorDailyAvailability>();

    public virtual ICollection<Patient> Patient { get; set; } = new List<Patient>();
}
