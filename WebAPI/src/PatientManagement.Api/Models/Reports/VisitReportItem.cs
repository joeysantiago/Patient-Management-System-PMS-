using PatientManagement.Api.Models.Consultations;

namespace PatientManagement.Api.Models.Reports;

public class VisitReportItem
{
    public Guid VisitId { get; set; }

    public Guid PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public DateTime VisitDateUtc { get; set; }

    public string Complaints { get; set; } = string.Empty;

    public string Diagnosis { get; set; } = string.Empty;

    public VitalsDto Vitals { get; set; } = new();

    public List<MedicationDto> Medications { get; set; } = new();
}
