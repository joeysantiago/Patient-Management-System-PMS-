namespace PatientManagement.Api.Data.Entities;

public class DoctorAccount
{
    public Guid Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTime UpdatedAtUtc { get; set; }
}
