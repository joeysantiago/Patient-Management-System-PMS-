using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Reports;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("visits")]
    public async Task<ActionResult<IReadOnlyList<VisitReportItem>>> SearchVisits(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? patientName, CancellationToken cancellationToken)
    {
        var results = await _reportService.SearchVisitsAsync(from, to, patientName, cancellationToken);
        return Ok(results);
    }

    [HttpGet("visits/export")]
    public async Task<IActionResult> ExportVisits(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromQuery] string? patientName, CancellationToken cancellationToken)
    {
        var export = await _reportService.ExportVisitsCsvAsync(from, to, patientName, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(export.CsvContent);
        return File(bytes, "text/csv", export.FileName);
    }
}
