namespace SehatMand.Application.Dto.Billing;

public record ReadBillingInfoDto(
    string Id,
    string AppointmentId,
    DateTime AppointmentDate,
    string PatientName,
    float PatientRating,
    decimal Amount,
    DateTime TransactionDate
    );