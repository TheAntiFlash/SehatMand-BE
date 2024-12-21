using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.Json.Serialization;

namespace SehatMand.Application.Dto.Appointment;

public record CreateAppointmentDto(
    [Required] string DoctorId,
    [Required] bool IsOnline,
    [Required] string AppointmentDate,
    [Required] string AppointmentTime
)
{
    [JsonIgnore]
    public DateTime InternalAppointmentTime =   DateTime
        .ParseExact(AppointmentDate, "MM/dd/yyyy", CultureInfo.InvariantCulture)
        .Add(TimeSpan.Parse(AppointmentTime));
   
    
}