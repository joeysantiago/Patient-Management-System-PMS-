using Microsoft.Extensions.Options;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class DatabaseBackupHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseBackupHostedService> _logger;
    private readonly TimeSpan _interval;

    public DatabaseBackupHostedService(
        IServiceProvider serviceProvider,
        IOptions<BackupSettings> settings,
        ILogger<DatabaseBackupHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _interval = TimeSpan.FromHours(Math.Max(1, settings.Value.IntervalHours));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunBackupAsync(stoppingToken);

            try
            {
                await Task.Delay(_interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunBackupAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var backupService = scope.ServiceProvider.GetRequiredService<IBackupService>();
            var backup = await backupService.RunBackupNowAsync(cancellationToken);
            _logger.LogInformation("Database backup created: {FileName}", backup.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Scheduled database backup failed.");
        }
    }
}
