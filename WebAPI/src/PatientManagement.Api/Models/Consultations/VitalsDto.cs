using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Consultations;

public class VitalsDto
{
    [Required]
    public decimal TemperatureCelsius { get; set; }

    [Required]
    public int BloodPressureSystolic { get; set; }

    [Required]
    public int BloodPressureDiastolic { get; set; }

    [Required]
    public int PulseBpm { get; set; }
}
