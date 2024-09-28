using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Appointment;

public record QueryAppointmentStatus(
    [RegularExpression("^pending|scheduled|completed|cancelled|rejected$",
        ErrorMessage = "Invalid status. Must be one of 'pending', 'scheduled', 'completed', 'cancelled', 'rejected'")]
    string? Status
    );