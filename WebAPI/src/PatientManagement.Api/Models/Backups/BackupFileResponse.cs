namespace PatientManagement.Api.Models.Backups;

public class BackupFileResponse
{
    public string FileName { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
