using System.Net.Http.Json;
using Amazon.Runtime.Internal.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SehatMand.Domain.Entities;
using SehatMand.Domain.Interface.Repository;
using SehatMand.Domain.Interface.Service;
using SehatMand.Infrastructure.Persistence;

namespace SehatMand.Infrastructure.Repository;

public class DoctorRepository(SmDbContext context, IStorageService storageServ, ILogger<DoctorRepository> logger): IDoctorRepository
{
    public async Task<List<Doctor>> GetAsync(string? name, string? speciality, List<string>? symptoms)
    {
        Task<HttpResponseMessage>? response = null;
        if (symptoms is { Count: > 0 })
        {
            var http = new HttpClient();
            http.BaseAddress = new Uri("http://api.sehatmand.live:8080");//Uri("http://localhost:8000");
            var request = new
            {
                symptoms = symptoms.ToArray()
            };
            
            response = http.PostAsJsonAsync("/recommendations", request);
        }
        
        var query = context.Doctor
            .Include(d => d.User)
            .Include(d => d.Qualifications)
            .Include(d => d.Speciality)
            .Include(d => d.Appointment)
            .ThenInclude(d => d.Review).AsQueryable();
        if (name != null)
        {
            query = query.Where(d => d.Name.ToLower().Contains(name.ToLower()));
        }
        if (speciality != null)
        {
            query = query.Where(d => d.Speciality.Value.ToLower().Contains(speciality.ToLower()));
        }

        if (response != null)
        {
            var res = await response;
            logger.LogCritical("Response: {0}", await res.Content.ReadAsStringAsync());
            var diagnosis = await (await response).Content.ReadFromJsonAsync<List<AiDiagnosis>>();
            diagnosis = diagnosis.OrderByDescending(a => a.Chance).ToList();
            var recommendedSpeciality = diagnosis.FirstOrDefault()?.Specialist ?? "";
            logger.LogInformation("Recommended Speciality: {0}", recommendedSpeciality);
            query = query.Where(d => d.Speciality.Value.ToLower().Contains(recommendedSpeciality.ToLower()));
        }
        

        return await query.OrderByDescending(d => d.Appointment
            .Where(a => a.Review.Count > 0)
            .Select(a => a.Review.Average(r => r.rating)).FirstOrDefault()).ToListAsync();
    }

    public async Task<Doctor?> GetByEmailAsync(string email)
    {
        return await context.Doctor.Include(d => d.User).FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<List<Doctor>> GetNearestDoctors(string? patientCity)
    {
        return await context.Doctor
            .Include(d => d.Qualifications)
            .Include(d => d.Appointment)
            .ThenInclude(d => d.Review)
            .Include(d => d.Speciality)
            .Where(d => d.City != null && d.City.Equals(patientCity, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(d => d.Appointment
                .Where(a => a.Review.Count > 0)
                .Select(a => a.Review.Average(r => r.rating)).FirstOrDefault())
            .ToListAsync();
    }
    
    public async Task<Doctor?> GetByIdAsync(string id)
    {
        return await context.Doctor
            .Include(d => d.DoctorDailyAvailability)
            .Include(d => d.Qualifications)
            .Include(d => d.Speciality)
            .Include(d => d.Appointment)
            .ThenInclude(d => d.Review)
            .Include(d => d.Appointment)
            .ThenInclude(a => a.patient)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
    
    public async Task UpdatePassword(string id, string oldPassword, string newPassword)
    {
        var doctor = await GetByUserIdAsync(id);
        Console.WriteLine(doctor);
        if (doctor == null) throw new Exception("Doctor Not Found");
        
        if (!BCrypt.Net.BCrypt.Verify(oldPassword, doctor.User.PasswordHash))
        {
            throw new Exception("Invalid Password");
        }
        
        doctor.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await context.SaveChangesAsync();
    }

    public Task<string?> GetDoctorIdByUserId(string id)
    {
        return context.Doctor.Where(d => d.UserId == id).Select(d => d.Id).FirstOrDefaultAsync();
    }

    public async Task<Doctor?> GetByUserIdAsync(string uid)
    {
        return await context.Doctor
            .Include(d => d.DoctorDailyAvailability)
            .Include(d => d.User)
            .Include(d => d.Qualifications)
            .Include(d => d.Speciality)
            .Include(d => d.Appointment)
            .ThenInclude(d => d.Review)
            .Include(d => d.Appointment)
            .ThenInclude(a => a.patient)
            .FirstOrDefaultAsync(d => d.UserId == uid);
    }

    public async Task UpdateProfile(string id, string? dtoCity, string? dtoAddress, string? dtoProfileInfo,
        IFormFile? dtoProfilePicture, string? dtoSpeciality,
        IEnumerable<DoctorAvailability>? availability, string? dtoPhone, string? dtoClinicId)
    {
        var doctor = await GetByUserIdAsync(id);
        if (doctor == null) throw new Exception("Doctor not found");

        if (dtoCity == null && dtoAddress == null && dtoProfileInfo == null && dtoSpeciality == null && availability == null && dtoPhone == null && dtoClinicId == null)
        {
            throw new Exception("No values provided to update");
        }

        if (dtoCity != null) doctor.City = dtoCity;
        if (dtoAddress != null) doctor.Address = dtoAddress;
        if (dtoProfileInfo != null) doctor.ProfileInfo = dtoProfileInfo;
        if (dtoSpeciality != null) doctor.SpecialityId = dtoSpeciality;
        if (availability != null)
        {
            foreach (var doctorAvailability in availability)
            {
                var availabilityFound =
                    doctor.DoctorDailyAvailability.FirstOrDefault(a => a.day_of_week == doctorAvailability.DayOfWeek);
                if (availabilityFound == null)
                {
                    doctor.DoctorDailyAvailability.Add(new DoctorDailyAvailability
                    {
                        day_of_week = doctorAvailability.DayOfWeek,
                        availability_start = doctorAvailability.AvailabilityStart,
                        availability_end = doctorAvailability.AvailabilityEnd
                    });
                }
                else
                {
                    availabilityFound.availability_start = doctorAvailability.AvailabilityStart;
                    availabilityFound.availability_end = doctorAvailability.AvailabilityEnd;
                
                }
            }
        }
        if (dtoPhone != null) doctor.Phone = dtoPhone;
        if (dtoClinicId != null) doctor.ClinicId = dtoClinicId;

        if (dtoProfilePicture != null)
        {
            if (dtoProfilePicture.Length > 2097152L) throw new Exception("File size too large");
            if (dtoProfilePicture.ContentType != "image/jpeg" &&
                dtoProfilePicture.ContentType != "image/png" &&
                dtoProfilePicture.ContentType != "image/jpg")
            {
                throw new Exception("Profile picture should be in jpeg/jpg or png format");
            }            
            //profile picture
            var basePath = Path.Join("doctor","profile-picture");
            /*if (doctor.ProfilePictureUrl != null) TODO uncomment this block before production
            {
                await storageServ.DeleteFileAsync(doctor.ProfilePictureUrl); // delete old profile picture
            }*/
            await storageServ.UploadFileAsync(dtoProfilePicture, doctor.Id, basePath); // upload
            
            doctor.ProfilePictureUrl = Path.Join(basePath, doctor.Id + Path.GetExtension(dtoProfilePicture.FileName)); // save path to db
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<Speciality>> GetSpecialities()
    {
        return await context.Speciality.ToListAsync();
    }

    public async Task<bool> AnyWithPmdcIdAsync(string pmdcId)
    {
       return await context.Doctor.AnyAsync(d => d.RegistrationId == pmdcId);
    }
}