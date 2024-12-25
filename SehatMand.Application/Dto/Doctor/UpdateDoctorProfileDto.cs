using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace SehatMand.Application.Dto.Doctor;

public record UpdateDoctorProfileDto(
    [MaxLength(7, ErrorMessage = "Max 7 possible.")]
    List<DoctorAvailabilityDto>? availabilities,
    [Phone]
    string? phone,
    IFormFile? profilePicture,
    string? clinicId,
    string? specialityId,
    string? address,
    string? profileInfo,
    string? city
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