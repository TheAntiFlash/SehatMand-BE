using System.ComponentModel.DataAnnotations;

namespace SehatMand.Application.Dto.Utils;

public record DateOnly(
    [Required] [Range(1900, 2100)] int Year,
    [Required] [Range(1, 12)] int Month,
    [Required] [Range(1, 31)] int Day
)
{
    public DateTime ToDateTime()
    {
        return new DateTime(Year, Month, Day);
    }
}

public static class DateOnlyExtensions
{
    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return new DateOnly(dateTime.Year, dateTime.Month, dateTime.Day);
    }
}
