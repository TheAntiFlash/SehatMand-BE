using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Appointment;

public record UpdateAppointmentStatusDto(
    [Required]
    [RegularExpression("^scheduled|rejected|cancelled|completed$")]
    string Status
    );