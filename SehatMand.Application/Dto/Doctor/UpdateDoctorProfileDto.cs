using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SehatMand.Application.Dto.Doctor;

public record UpdateDoctorProfileDto(
    [MaxLength(7, ErrorMessage = "Max 7 possible.")]
    List<DoctorAvailabilityDto>? Availabilities,
    [Phone]
    string? Phone,
    string? ClinicId,
    string? SpecialityId,
    string? Address,
    string? ProfileInfo,
    string? City
    );

public record DoctorAvailabilityDto(
    [Range(0,6, ErrorMessage = "Invalid day of week.")]
    [Required]
    int DayOfWeek,
    [Required]
    string StartTime,
    [Required]
    string EndTime
)
{
    [JsonIgnore] public TimeSpan StartTimeInternal { get; init;} =  TimeSpan.Parse(StartTime);
    [JsonIgnore] public TimeSpan EndTimeInternal { get; init; } =  TimeSpan.Parse(EndTime);

}