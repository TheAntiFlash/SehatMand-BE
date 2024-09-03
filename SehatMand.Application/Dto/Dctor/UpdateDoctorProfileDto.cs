using System.Text.Json.Serialization;

namespace SehatMand.Application.Dto.Dctor;

public record UpdateDoctorProfileDto(
    
    List<UpdateDoctorAvailability>? Availabilities,
    string? Email,
    string? Phone,
    string? ClinicId
    );

public record UpdateDoctorAvailability(
    int? DayOfWeek,
    string? StartTime,
    string? EndTime
)
{
    [JsonIgnore] public TimeSpan? StartTimeInternal { get; } = StartTime is not null? TimeSpan.Parse(StartTime) : null;
    [JsonIgnore] public TimeSpan? EndTimeInternal { get; } = EndTime is not null? TimeSpan.Parse(EndTime) : null;

}