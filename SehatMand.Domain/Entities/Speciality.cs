namespace SehatMand.Domain.Entities;

public class Speciality
{
   public string Id { get; set; } = Guid.NewGuid().ToString();
   public string Value { get; set; } = null!;
   
   public virtual List<Doctor> Doctors { get; set; } = new List<Doctor>();
}