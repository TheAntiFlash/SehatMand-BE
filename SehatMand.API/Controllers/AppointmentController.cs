using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Appointment;

namespace SehatMand.API.Controllers;

[ApiController]
[Route("api/appointment")]
public class AppointmentController: ControllerBase
{
    [Authorize]
    [HttpGet]
    [Route("scheduled")]
    public async Task<IActionResult> GetAppointments()
    {
        var appointments = new List<ReadAppointmentDto>
        {
            new (
                "dummyId-of-36-characters-love-uuid-1",
                "Ahmed Bashir",
                "Surgeon",
                DateTime.Now.Date,
                DateTime.Now.TimeOfDay.Add(new TimeSpan(2,0,0))
            ),
            new (
                "dummyId-of-36-characters-love-uuid-2",
                "Mr. Doctor",
                "Dentist",
                DateTime.Now.Date.AddDays(1),
                DateTime.Now.TimeOfDay.Add(new TimeSpan(4,0,0))
            )

        };

        return Ok(appointments);
    }
    
}