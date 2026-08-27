using PatientManagement.Api.Models.Appointments;

namespace PatientManagement.Api.Services.IService;

public interface IAppointmentService
{
    Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(DateOnly? date, CancellationToken cancellationToken);

    Task<AppointmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AppointmentCreateResult> CreateAsync(AppointmentCreateRequest request, CancellationToken cancellationToken);

    Task<AppointmentUpdateResult> UpdateAsync(Guid id, AppointmentUpdateRequest request, CancellationToken cancellationToken);

    Task<(string FileName, string CsvContent)> ExportCsvAsync(DateOnly? date, CancellationToken cancellationToken);
}
