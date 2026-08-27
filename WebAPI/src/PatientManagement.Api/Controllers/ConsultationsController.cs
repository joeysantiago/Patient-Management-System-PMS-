using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Consultations;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ConsultationsController : ControllerBase
{
    private readonly IConsultationService _consultationService;

    public ConsultationsController(IConsultationService consultationService)
    {
        _consultationService = consultationService;
    }

    [HttpGet("by-appointment/{appointmentId:guid}")]
    public async Task<ActionResult<VisitResponse>> GetByAppointment(Guid appointmentId, CancellationToken cancellationToken)
    {
        var visit = await _consultationService.GetByAppointmentIdAsync(appointmentId, cancellationToken);
        return visit is null ? NotFound() : Ok(visit);
    }

    [HttpGet("by-appointment/{appointmentId:guid}/prescription")]
    public async Task<ActionResult<PrescriptionResponse>> GetPrescription(Guid appointmentId, CancellationToken cancellationToken)
    {
        var prescription = await _consultationService.GetPrescriptionAsync(appointmentId, cancellationToken);
        return prescription is null ? NotFound() : Ok(prescription);
    }

    [HttpGet("by-appointment/{appointmentId:guid}/prescription/pdf")]
    public async Task<IActionResult> ExportPrescriptionPdf(Guid appointmentId, CancellationToken cancellationToken)
    {
        var export = await _consultationService.ExportPrescriptionPdfAsync(appointmentId, cancellationToken);
        return export is null ? NotFound() : File(export.Value.Bytes, "application/pdf", export.Value.FileName);
    }

    [HttpPost]
    public async Task<ActionResult<VisitResponse>> Save([FromBody] VisitCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _consultationService.CreateOrUpdateVisitAsync(request, cancellationToken);

        return result.Status switch
        {
            ConsultationSaveStatus.Success => Ok(result.Visit),
            ConsultationSaveStatus.AppointmentNotFound => NotFound(new { message = "Appointment not found." }),
            ConsultationSaveStatus.AppointmentCancelledOrNoShow => BadRequest(new { message = "This appointment is cancelled or marked no-show and cannot be consulted." }),
            _ => Problem(),
        };
    }
}
