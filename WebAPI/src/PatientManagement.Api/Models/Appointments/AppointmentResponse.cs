using PatientManagement.Api.Data.Entities;

namespace PatientManagement.Api.Models.Appointments;

public class AppointmentResponse
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientPhoneNumber { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    public AppointmentStatus Status { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
