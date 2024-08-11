namespace SehatMand.Application.Dto.PmcDoctor;

public record PmcDoctor(
    bool Status,
    PmcData Data, 
    string Message 
        );
        
public record PmcData(

    string RegistrationNo,
    string Name,
    string FatherName,
    string? Gender, // This is a bug in the API response, gender is always null
    string RegistrationType,
    string RegistrationDate,
    string ValidUpto,
    string Status,
    bool IsFaculty,
    List<PmcQualification> Qualifications);
    
public record PmcQualification(
    string Speciality,
    string Degree,
    string University,
    string PassingYear
);