using System.Text;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Reports;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class ReportService : IReportService
{
    private readonly AppDbContext _dbContext;

    public ReportService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<VisitReportItem>> SearchVisitsAsync(DateOnly? from, DateOnly? to, string? patientName, CancellationToken cancellationToken)
    {
        var visits = await GetVisitsAsync(from, to, patientName, cancellationToken);
        return visits.Select(ToReportItem).ToList();
    }

    public async Task<(string FileName, string CsvContent)> ExportVisitsCsvAsync(DateOnly? from, DateOnly? to, string? patientName, CancellationToken cancellationToken)
    {
        var visits = await GetVisitsAsync(from, to, patientName, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("PatientName,VisitDate,Complaints,Diagnosis,TemperatureCelsius,BPSystolic,BPDiastolic,PulseBpm,MedicationName,Dosage,Frequency,Duration,Instructions");

        foreach (var visit in visits)
        {
            var item = ToReportItem(visit);
            var commonFields = new[]
            {
                CsvEscape(item.PatientName),
                CsvEscape(item.VisitDateUtc.ToString("yyyy-MM-dd HH:mm")),
                CsvEscape(item.Complaints),
                CsvEscape(item.Diagnosis),
                CsvEscape(item.Vitals.TemperatureCelsius.ToString()),
                CsvEscape(item.Vitals.BloodPressureSystolic.ToString()),
                CsvEscape(item.Vitals.BloodPressureDiastolic.ToString()),
                CsvEscape(item.Vitals.PulseBpm.ToString()),
            };

            if (item.Medications.Count == 0)
            {
                csv.AppendLine(string.Join(',', commonFields.Concat(new[] { CsvEscape(null), CsvEscape(null), CsvEscape(null), CsvEscape(null), CsvEscape(null) })));
                continue;
            }

            foreach (var medication in item.Medications)
            {
                var row = commonFields.Concat(new[]
                {
                    CsvEscape(medication.Name),
                    CsvEscape(medication.Dosage),
                    CsvEscape(medication.Frequency),
                    CsvEscape(medication.Duration),
                    CsvEscape(medication.Instructions),
                });
                csv.AppendLine(string.Join(',', row));
            }
        }

        var fileName = $"visit-report-{DateTime.UtcNow:yyyyMMdd}.csv";
        return (fileName, csv.ToString());
    }

    private async Task<List<Visit>> GetVisitsAsync(DateOnly? from, DateOnly? to, string? patientName, CancellationToken cancellationToken)
    {
        var query = _dbContext.Visits
            .AsNoTracking()
            .Include(v => v.Patient)
            .Include(v => v.Vitals)
            .Include(v => v.Medications)
            .AsQueryable();

        if (from.HasValue)
        {
            var start = from.Value.ToDateTime(TimeOnly.MinValue);
            query = query.Where(v => v.VisitDateUtc >= start);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            query = query.Where(v => v.VisitDateUtc < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(patientName))
        {
            var term = patientName.Trim();
            query = query.Where(v => v.Patient != null && EF.Functions.Like(v.Patient.FullName, $"%{term}%"));
        }

        return await query.OrderByDescending(v => v.VisitDateUtc).ToListAsync(cancellationToken);
    }

    private static VisitReportItem ToReportItem(Visit visit)
    {
        var mapped = VisitMapper.ToResponse(visit);

        return new VisitReportItem
        {
            VisitId = mapped.Id,
            PatientId = mapped.PatientId,
            PatientName = visit.Patient?.FullName ?? string.Empty,
            VisitDateUtc = mapped.VisitDateUtc,
            Complaints = mapped.Complaints,
            Diagnosis = mapped.Diagnosis,
            Vitals = mapped.Vitals,
            Medications = mapped.Medications,
        };
    }

    private static string CsvEscape(string? value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }
}
