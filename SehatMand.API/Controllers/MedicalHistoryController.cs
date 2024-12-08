using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SehatMand.Application.Dto.Error;
using SehatMand.Application.Dto.MedialHistoryDocument;
using SehatMand.Application.Mapper;
using SehatMand.Domain.Interface.Repository;

namespace SehatMand.API.Controllers;

/// <summary>
/// Medical history controller
/// </summary>
/// <param name="repo"></param>
/// <param name="logger"></param>
[ApiController]
[Route("api/medical-history")]
public class MedicalHistoryController(
    IMedicalHistoryRepository repo,
    ILogger<MedicalHistoryController> logger,
    IPatientRepository patientRepo,
    IAppointmentRepository appointmentRepo) : ControllerBase
{
    /// <summary>
    /// Add medical history document
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddMedicalHistory([FromForm] CreateMedicalHistoryByPatientDocument request)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patientId = await patientRepo.GetPatientIdByUserId(id);
            if (patientId == null) throw new Exception("Patient not found");
            var docId = await repo.AddMedicalHistoryDocumentAsync(request.ToHistoryDocument(patientId, id), request.File, "patient");
            return CreatedAtAction(nameof(AddMedicalHistory), docId, new
            {
                Success = true,
                Message = "Medical history document added successfully" 
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while adding medical history document");
            return StatusCode(500);
        }
    }
    
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMedicalHistoryDocuments()
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patientId = await patientRepo.GetPatientIdByUserId(id);
            if (patientId == null) throw new Exception("Patient not found");
            var documents = await repo.GetMedicalHistoryDocumentsByPatientIdAsync(patientId);
            return Ok(documents.Select(d => d.ToReadMedicalHistoryDocument()));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting medical history documents");
            return StatusCode(500);
        }
    }
    
    /// <summary>
    /// Get medical history document by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetMedicalHistoryDocumentById([FromRoute] string id)
    {
        try
        {
            var document = await repo.GetMedicalHistoryDocumentByIdAsync(id);
            if (document == null) throw new Exception("Document not found");
            logger.LogInformation(document.Headers.ContentType);
            return File(document.ResponseStream, document.Headers.ContentType);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while getting medical history document");
            return StatusCode(500);
        }
    }
    
    /// <summary>
    /// Add medical history document for appointment by doctor
    /// </summary>
    /// <param name="request"></param>
    /// <param name="appointmentId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [Authorize]
    [HttpPost]
    [Route("{appointmentId}")]
    public async Task<IActionResult> AddMedicalHistoryForAppointment([FromForm] CreateMedicalHistoryByDoctor request, [FromRoute] string appointmentId)
    {
        try
        {
            if (HttpContext.User.Identity is not ClaimsIdentity identity)
                return BadRequest(new ResponseDto("Error", "Something went wrong"));
        
            var claims = identity.Claims;
            var id = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            if (id == null) throw new Exception("User not found");
            var patientId = (await appointmentRepo.GetAppointmentByIdAsync(appointmentId))?.patient_id;
            if (patientId == null) throw new Exception("Patient not found");
            var docId = await repo.AddMedicalHistoryDocumentAsync(request.ToHistoryDocument(appointmentId, patientId, id), request.File, "doctors-notes");
            return CreatedAtAction(nameof(AddMedicalHistoryForAppointment), docId, new
            {
                Success = true,
                Message = "Medical history document added successfully" 
            });
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while adding medical history document");
            return StatusCode(500);
        }
    }
}