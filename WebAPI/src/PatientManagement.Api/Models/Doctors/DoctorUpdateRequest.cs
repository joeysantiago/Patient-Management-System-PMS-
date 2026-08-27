using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Doctors;

public class DoctorUpdateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Specialization { get; set; } = string.Empty;

    [StringLength(50)]
    public string? RegistrationNumber { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    [StringLength(300)]
    public string? ClinicAddress { get; set; }
}
