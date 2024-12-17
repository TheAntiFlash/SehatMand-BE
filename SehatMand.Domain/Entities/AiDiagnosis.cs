namespace SehatMand.Domain.Entities;

public record AiDiagnosis(
    string Disease,
    double Chance,
    string Specialist
    );