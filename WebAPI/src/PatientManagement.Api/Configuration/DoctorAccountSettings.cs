namespace PatientManagement.Api.Configuration;

/// <summary>
/// Phase 1 is single-user only: the one doctor account is configured here
/// instead of a Users table. Username/PasswordHash live in appsettings.
/// </summary>
public class DoctorAccountSettings
{
    public const string SectionName = "DoctorAccount";

    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
