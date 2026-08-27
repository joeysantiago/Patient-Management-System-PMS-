using System.ComponentModel.DataAnnotations;
using PatientManagement.Api.Data.Entities;

namespace PatientManagement.Api.Models.Appointments;

public class AppointmentUpdateRequest
{
    [Required]
    public Guid PatientId { get; set; }

    [Required]
    public DateTime ScheduledAt { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}
