namespace PatientManagement.Api.Models.Consultations;

public class VisitResponse
{
    public Guid Id { get; set; }

    public Guid PatientId { get; set; }

    public Guid AppointmentId { get; set; }

    public DateTime VisitDateUtc { get; set; }

    public VitalsDto Vitals { get; set; } = new();

    public string Complaints { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public List<MedicationDto> Medications { get; set; } = new();

    public bool VitalsOutOfRange { get; set; }

    public string? VitalsWarning { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }
}
