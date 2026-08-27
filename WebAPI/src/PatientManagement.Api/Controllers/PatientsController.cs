using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Patients;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PatientResponse>>> GetAll([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var patients = await _patientService.GetAllAsync(search, cancellationToken);
        return Ok(patients);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var export = await _patientService.ExportListCsvAsync(search, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(export.CsvContent);
        return File(bytes, "text/csv", export.FileName);
    }

    [HttpGet("{id:guid}/export/pdf")]
    public async Task<IActionResult> ExportRecordPdf(Guid id, CancellationToken cancellationToken)
    {
        var export = await _patientService.ExportRecordPdfAsync(id, cancellationToken);
        return export is null ? NotFound() : File(export.Value.Bytes, "application/pdf", export.Value.FileName);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _patientService.GetByIdAsync(id, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpPost]
    public async Task<ActionResult<PatientResponse>> Create([FromBody] PatientCreateRequest request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PatientResponse>> Update(Guid id, [FromBody] PatientUpdateRequest request, CancellationToken cancellationToken)
    {
        var patient = await _patientService.UpdateAsync(id, request, cancellationToken);
        return patient is null ? NotFound() : Ok(patient);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<PatientHistoryResponse>> GetHistory(Guid id, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var history = await _patientService.GetHistoryAsync(id, from, to, cancellationToken);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpGet("{id:guid}/history/export")]
    public async Task<IActionResult> ExportHistory(Guid id, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken)
    {
        var export = await _patientService.ExportHistoryCsvAsync(id, from, to, cancellationToken);
        if (export is null)
            return NotFound();

        var bytes = System.Text.Encoding.UTF8.GetBytes(export.Value.CsvContent);
        return File(bytes, "text/csv", export.Value.FileName);
    }
}
