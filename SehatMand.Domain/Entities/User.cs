namespace SehatMand.Domain.Entities;

public partial class User
{
    public string id { get; set; } = null!;

    public string username { get; set; } = null!;

    public string email { get; set; } = null!;

    public string password_hash { get; set; } = null!;

    public string role { get; set; } = null!;

    public bool is_active { get; set; }

    public virtual ICollection<Clinic> Cliniccreated_byNavigation { get; set; } = new List<Clinic>();

    public virtual ICollection<Clinic> Clinicmodified_byNavigation { get; set; } = new List<Clinic>();

    public virtual ICollection<Coupon> Couponcreated_byNavigation { get; set; } = new List<Coupon>();

    public virtual ICollection<Coupon> Couponused_byNavigation { get; set; } = new List<Coupon>();

    public virtual ICollection<Doctor> Doctor { get; set; } = new List<Doctor>();

    public virtual ICollection<DoctorDailyAvailability> DoctorDailyAvailability { get; set; } = new List<DoctorDailyAvailability>();

    public virtual ICollection<Patient> Patient { get; set; } = new List<Patient>();
}
