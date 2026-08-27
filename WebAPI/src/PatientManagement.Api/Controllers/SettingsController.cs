using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Settings;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IClinicSettingsService _clinicSettingsService;

    public SettingsController(IClinicSettingsService clinicSettingsService)
    {
        _clinicSettingsService = clinicSettingsService;
    }

    [HttpGet("clinic")]
    public async Task<ActionResult<ClinicSettingsResponse>> GetClinicSettings(CancellationToken cancellationToken)
    {
        var settings = await _clinicSettingsService.GetAsync(cancellationToken);
        return Ok(settings);
    }

    [HttpPut("clinic")]
    public async Task<ActionResult<ClinicSettingsResponse>> UpdateClinicSettings([FromBody] ClinicSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        var settings = await _clinicSettingsService.UpdateAsync(request, cancellationToken);
        return Ok(settings);
    }
}
