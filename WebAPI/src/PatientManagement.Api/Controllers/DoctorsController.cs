using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Doctors;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DoctorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var doctors = await _doctorService.GetAllAsync(cancellationToken);
        return Ok(doctors);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DoctorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.GetByIdAsync(id, cancellationToken);
        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpPost]
    public async Task<ActionResult<DoctorResponse>> Create([FromBody] DoctorCreateRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DoctorResponse>> Update(Guid id, [FromBody] DoctorUpdateRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _doctorService.UpdateAsync(id, request, cancellationToken);
        return doctor is null ? NotFound() : Ok(doctor);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _doctorService.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
