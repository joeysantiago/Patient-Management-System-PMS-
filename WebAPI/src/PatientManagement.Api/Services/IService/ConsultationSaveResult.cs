using PatientManagement.Api.Models.Consultations;

namespace PatientManagement.Api.Services.IService;

public enum ConsultationSaveStatus
{
    Success,
    AppointmentNotFound,
    AppointmentCancelledOrNoShow,
}

public class ConsultationSaveResult
{
    public required ConsultationSaveStatus Status { get; init; }

    public VisitResponse? Visit { get; init; }
}
