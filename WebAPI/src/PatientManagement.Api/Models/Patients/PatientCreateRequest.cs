using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Patients;

public class PatientCreateRequest
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required]
    [StringLength(20)]
    public string Gender { get; set; } = string.Empty;

    [Required]
    [Phone]
    [StringLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Address { get; set; }
}
