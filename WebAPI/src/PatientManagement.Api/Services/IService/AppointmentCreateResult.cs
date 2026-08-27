using PatientManagement.Api.Models.Appointments;

namespace PatientManagement.Api.Services.IService;

public enum AppointmentCreateStatus
{
    Success,
    PatientNotFound,
    TimeSlotAlreadyBooked,
    PatientAlreadyScheduledOnDate,
}

public class AppointmentCreateResult
{
    public required AppointmentCreateStatus Status { get; init; }

    public AppointmentResponse? Appointment { get; init; }
}
