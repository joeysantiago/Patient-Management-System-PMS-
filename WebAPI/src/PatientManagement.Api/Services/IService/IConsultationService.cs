using PatientManagement.Api.Models.Consultations;

namespace PatientManagement.Api.Services.IService;

public interface IConsultationService
{
    Task<VisitResponse?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken);

    Task<ConsultationSaveResult> CreateOrUpdateVisitAsync(VisitCreateRequest request, CancellationToken cancellationToken);

    Task<PrescriptionResponse?> GetPrescriptionAsync(Guid appointmentId, CancellationToken cancellationToken);

    Task<(string FileName, byte[] Bytes)?> ExportPrescriptionPdfAsync(Guid appointmentId, CancellationToken cancellationToken);
}
