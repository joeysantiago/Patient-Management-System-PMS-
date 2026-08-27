using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Consultations;

public class VisitCreateRequest
{
    [Required]
    public Guid AppointmentId { get; set; }

    [Required]
    public VitalsDto Vitals { get; set; } = new();

    [Required]
    [StringLength(2000)]
    public string Complaints { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Diagnosis { get; set; } = string.Empty;

    public List<MedicationDto> Medications { get; set; } = new();
}
