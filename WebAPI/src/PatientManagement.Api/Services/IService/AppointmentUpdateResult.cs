namespace PatientManagement.Api.Services.IService;

public enum AppointmentUpdateResult
{
    Success,
    AppointmentNotFound,
    PatientNotFound,
    TimeSlotAlreadyBooked,
}
