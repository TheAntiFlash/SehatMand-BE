namespace SehatMand.Domain.Entities;

public partial class Qualification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string Speciality { get; set; } = null!;

    public string Degree { get; set; } = null!;

    public string University { get; set; } = null!;
    
    public DateTime PassingYear { get; set; }

    public List<Doctor> Doctors { get; } = [];
}