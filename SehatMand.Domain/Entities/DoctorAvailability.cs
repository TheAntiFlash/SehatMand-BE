namespace SehatMand.Domain.Entities;

public record DoctorAvailability(
    int DayOfWeek,
    TimeSpan AvailabilityStart,
    TimeSpan AvailabilityEnd
);