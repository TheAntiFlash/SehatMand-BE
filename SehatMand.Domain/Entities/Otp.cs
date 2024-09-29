namespace SehatMand.Domain.Entities;

public record Otp(
    string? Value,
    DateTime? Expiry
    );