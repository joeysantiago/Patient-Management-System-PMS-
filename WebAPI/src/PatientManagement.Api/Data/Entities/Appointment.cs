namespace PatientManagement.Api.Data.Entities;

public class Appointment
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Patient? Patient { get; set; }

    public DateTime ScheduledAt { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
