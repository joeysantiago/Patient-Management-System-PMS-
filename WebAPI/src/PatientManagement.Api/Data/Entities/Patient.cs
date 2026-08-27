namespace PatientManagement.Api.Data.Entities;

public class Patient
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }

    public string Gender { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string? Address { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
