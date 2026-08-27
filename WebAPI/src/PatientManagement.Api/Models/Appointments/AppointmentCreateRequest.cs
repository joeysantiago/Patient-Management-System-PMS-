using System.ComponentModel.DataAnnotations;

namespace PatientManagement.Api.Models.Appointments;

public class AppointmentCreateRequest
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
