namespace PatientManagement.Api.Data.Entities;

public class Visit
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Patient? Patient { get; set; }

    public Guid AppointmentId { get; set; }

    public Appointment? Appointment { get; set; }

    public string Complaints { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public DateTime VisitDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public Vitals? Vitals { get; set; }

    public ICollection<Medication> Medications { get; set; } = new List<Medication>();
}
