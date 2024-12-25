namespace SehatMand.Application.Dto.Billing;

public record ReadBillingInfoDto(
    string Id,
    string AppointmentId,
    string PatientName,
    decimal Amount,
    DateTime TransactionDate
    );