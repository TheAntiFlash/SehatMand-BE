using SehatMand.Application.Dto.Speciality;
using SehatMand.Domain.Entities;

namespace SehatMand.Application.Mapper;

public static class SpecialityMapper
{
    public static ReadSpecialityDto ToReadSpecialityDto(this Speciality speciality)
    {
        return new ReadSpecialityDto(speciality.Id, speciality.Value);
    }
}