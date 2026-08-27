namespace PatientManagement.Api.Models.Doctors;

public class DoctorResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Specialization { get; set; } = string.Empty;

    public string? RegistrationNumber { get; set; }

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? ClinicAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
