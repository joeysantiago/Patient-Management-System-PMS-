using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientManagement.Api.Models.Backups;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BackupsController : ControllerBase
{
    private readonly IBackupService _backupService;

    public BackupsController(IBackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BackupFileResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var backups = await _backupService.ListBackupsAsync(cancellationToken);
        return Ok(backups);
    }

    [HttpPost("run")]
    public async Task<ActionResult<BackupFileResponse>> RunNow(CancellationToken cancellationToken)
    {
        var backup = await _backupService.RunBackupNowAsync(cancellationToken);
        return Ok(backup);
    }
}
