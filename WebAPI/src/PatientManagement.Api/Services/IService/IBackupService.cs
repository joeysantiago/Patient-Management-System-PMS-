using PatientManagement.Api.Models.Backups;

namespace PatientManagement.Api.Services.IService;

public interface IBackupService
{
    Task<IReadOnlyList<BackupFileResponse>> ListBackupsAsync(CancellationToken cancellationToken);

    Task<BackupFileResponse> RunBackupNowAsync(CancellationToken cancellationToken);
}
