using PatientManagement.Api.Models.Patients;

namespace PatientManagement.Api.Services.IService;

public interface IPatientService
{
    Task<IReadOnlyList<PatientResponse>> GetAllAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<PatientResponse>> GetRecentAsync(int count, CancellationToken cancellationToken);

    Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PatientResponse> CreateAsync(PatientCreateRequest request, CancellationToken cancellationToken);

    Task<PatientResponse?> UpdateAsync(Guid id, PatientUpdateRequest request, CancellationToken cancellationToken);

    Task<PatientHistoryResponse?> GetHistoryAsync(Guid patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);

    Task<(string FileName, string CsvContent)?> ExportHistoryCsvAsync(Guid patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);

    Task<(string FileName, string CsvContent)> ExportListCsvAsync(string? search, CancellationToken cancellationToken);

    Task<(string FileName, byte[] Bytes)?> ExportRecordPdfAsync(Guid patientId, CancellationToken cancellationToken);
}
