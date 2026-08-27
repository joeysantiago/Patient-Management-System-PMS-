using PatientManagement.Api.Models.Reports;

namespace PatientManagement.Api.Services.IService;

public interface IReportService
{
    Task<IReadOnlyList<VisitReportItem>> SearchVisitsAsync(DateOnly? from, DateOnly? to, string? patientName, CancellationToken cancellationToken);

    Task<(string FileName, string CsvContent)> ExportVisitsCsvAsync(DateOnly? from, DateOnly? to, string? patientName, CancellationToken cancellationToken);
}
