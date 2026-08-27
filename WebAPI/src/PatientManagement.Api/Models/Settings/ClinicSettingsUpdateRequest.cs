using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Settings;

public class ClinicSettingsUpdateRequest
{
    [Required]
    [StringLength(200)]
    public string ClinicName { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string DoctorName { get; set; } = string.Empty;

    [StringLength(50)]
    public string? RegistrationNumber { get; set; }

    [StringLength(150)]
    public string? Qualification { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(300)]
    public string? FooterNote { get; set; }
}
