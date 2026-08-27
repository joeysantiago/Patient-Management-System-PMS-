using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Consultations;

public class MedicationDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Dosage { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Frequency { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Duration { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Instructions { get; set; }
}
