namespace PatientManagement.Api.Models.Patients;

public class PatientResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public int Age { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
