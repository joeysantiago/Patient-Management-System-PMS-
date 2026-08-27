using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Appointments;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AppointmentResponse>>> GetAll([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var appointments = await _appointmentService.GetAllAsync(date, cancellationToken);
        return Ok(appointments);
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] DateOnly? date, CancellationToken cancellationToken)
    {
        var export = await _appointmentService.ExportCsvAsync(date, cancellationToken);
        var bytes = System.Text.Encoding.UTF8.GetBytes(export.CsvContent);
        return File(bytes, "text/csv", export.FileName);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _appointmentService.GetByIdAsync(id, cancellationToken);
        return appointment is null ? NotFound() : Ok(appointment);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create([FromBody] AppointmentCreateRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            AppointmentCreateStatus.Success => CreatedAtAction(nameof(GetById), new { id = result.Appointment!.Id }, result.Appointment),
            AppointmentCreateStatus.PatientNotFound => BadRequest(new { message = "The selected patient does not exist." }),
            AppointmentCreateStatus.TimeSlotAlreadyBooked => Conflict(new { message = "Another appointment is already scheduled within 15 minutes of this time." }),
            AppointmentCreateStatus.PatientAlreadyScheduledOnDate => Conflict(new { message = "This patient already has a scheduled appointment on this date." }),
            _ => Problem(),
        };
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AppointmentUpdateRequest request, CancellationToken cancellationToken)
    {
        var result = await _appointmentService.UpdateAsync(id, request, cancellationToken);

        return result switch
        {
            AppointmentUpdateResult.Success => NoContent(),
            AppointmentUpdateResult.AppointmentNotFound => NotFound(),
            AppointmentUpdateResult.PatientNotFound => BadRequest(new { message = "The selected patient does not exist." }),
            AppointmentUpdateResult.TimeSlotAlreadyBooked => Conflict(new { message = "Another appointment is already scheduled within 15 minutes of this time." }),
            _ => Problem(),
        };
    }
}
