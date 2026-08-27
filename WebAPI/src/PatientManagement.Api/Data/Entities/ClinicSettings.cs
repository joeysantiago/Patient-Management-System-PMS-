namespace PatientManagement.Api.Data.Entities;

public class ClinicSettings
{
    public Guid Id { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    public string DoctorName { get; set; } = string.Empty;

    public string? RegistrationNumber { get; set; }

    public string? Qualification { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? FooterNote { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
