namespace PatientManagement.Api.Configuration;

public class BackupSettings
{
    public const string SectionName = "Backup";

    public int IntervalHours { get; set; } = 24;

    public int RetentionCount { get; set; } = 7;

    public string Directory { get; set; } = "Backups";
}
