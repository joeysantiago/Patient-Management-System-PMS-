using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Dashboard;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryResponse>> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }
}
