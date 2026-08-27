using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Models.Backups;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class BackupService : IBackupService
{
    private readonly BackupSettings _settings;
    private readonly string _databaseFilePath;
    private readonly string _backupDirectory;

    public BackupService(IConfiguration configuration, IOptions<BackupSettings> settings, IHostEnvironment environment)
    {
        _settings = settings.Value;

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Default connection string is missing.");
        var builder = new SqliteConnectionStringBuilder(connectionString);
        _databaseFilePath = Path.IsPathRooted(builder.DataSource)
            ? builder.DataSource
            : Path.Combine(environment.ContentRootPath, builder.DataSource);

        _backupDirectory = Path.Combine(environment.ContentRootPath, _settings.Directory);
    }

    public Task<IReadOnlyList<BackupFileResponse>> ListBackupsAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(_backupDirectory))
            return Task.FromResult<IReadOnlyList<BackupFileResponse>>(new List<BackupFileResponse>());

        var files = new DirectoryInfo(_backupDirectory)
            .GetFiles("*.db")
            .OrderByDescending(f => f.CreationTimeUtc)
            .Select(f => new BackupFileResponse
            {
                FileName = f.Name,
                SizeBytes = f.Length,
                CreatedAtUtc = f.CreationTimeUtc,
            })
            .ToList();

        return Task.FromResult<IReadOnlyList<BackupFileResponse>>(files);
    }

    public Task<BackupFileResponse> RunBackupNowAsync(CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_backupDirectory);

        var fileName = $"patientmanagement-{DateTime.UtcNow:yyyyMMddHHmmss}.db";
        var destinationPath = Path.Combine(_backupDirectory, fileName);

        File.Copy(_databaseFilePath, destinationPath, overwrite: true);

        PruneOldBackups();

        var info = new FileInfo(destinationPath);
        return Task.FromResult(new BackupFileResponse
        {
            FileName = info.Name,
            SizeBytes = info.Length,
            CreatedAtUtc = info.CreationTimeUtc,
        });
    }

    private void PruneOldBackups()
    {
        var files = new DirectoryInfo(_backupDirectory)
            .GetFiles("*.db")
            .OrderByDescending(f => f.CreationTimeUtc)
            .ToList();

        foreach (var file in files.Skip(_settings.RetentionCount))
        {
            file.Delete();
        }
    }
}
