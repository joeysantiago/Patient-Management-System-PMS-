using System.Text;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Patients;
using PatientManagement.Api.Services.IService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PatientManagement.Api.Services.Service;

public class PatientService : IPatientService
{
    private readonly AppDbContext _dbContext;

    public PatientService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<PatientResponse>> GetAllAsync(string? search, CancellationToken cancellationToken)
    {
        var query = _dbContext.Patients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.FullName, $"%{term}%") || EF.Functions.Like(p.PhoneNumber, $"%{term}%"));
        }

        var patients = await query
            .OrderBy(p => p.FullName)
            .ToListAsync(cancellationToken);

        return patients.Select(ToResponse).ToList();
    }

    public async Task<IReadOnlyList<PatientResponse>> GetRecentAsync(int count, CancellationToken cancellationToken)
    {
        var patients = await _dbContext.Patients
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(count)
            .ToListAsync(cancellationToken);

        return patients.Select(ToResponse).ToList();
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        return patient is null ? null : ToResponse(patient);
    }

    public async Task<PatientResponse> CreateAsync(PatientCreateRequest request, CancellationToken cancellationToken)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Patients.Add(patient);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(patient);
    }

    public async Task<PatientResponse?> UpdateAsync(Guid id, PatientUpdateRequest request, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (patient is null)
            return null;

        patient.FullName = request.FullName.Trim();
        patient.DateOfBirth = request.DateOfBirth;
        patient.Gender = request.Gender.Trim();
        patient.PhoneNumber = request.PhoneNumber.Trim();
        patient.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(patient);
    }

    public async Task<PatientHistoryResponse?> GetHistoryAsync(Guid patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (patient is null)
            return null;

        var visits = await GetVisitsAsync(patientId, from, to, cancellationToken);

        return new PatientHistoryResponse
        {
            PatientId = patient.Id,
            PatientName = patient.FullName,
            Visits = visits.Select(VisitMapper.ToResponse).ToList(),
        };
    }

    public async Task<(string FileName, string CsvContent)?> ExportHistoryCsvAsync(Guid patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (patient is null)
            return null;

        var visits = await GetVisitsAsync(patientId, from, to, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("VisitDate,Complaints,Diagnosis,TemperatureCelsius,BPSystolic,BPDiastolic,PulseBpm,VitalsWarning,MedicationName,Dosage,Frequency,Duration,Instructions");

        foreach (var visit in visits)
        {
            var response = VisitMapper.ToResponse(visit);
            var commonFields = new[]
            {
                CsvEscape(response.VisitDateUtc.ToString("yyyy-MM-dd HH:mm")),
                CsvEscape(response.Complaints),
                CsvEscape(response.Diagnosis),
                CsvEscape(response.Vitals.TemperatureCelsius.ToString()),
                CsvEscape(response.Vitals.BloodPressureSystolic.ToString()),
                CsvEscape(response.Vitals.BloodPressureDiastolic.ToString()),
                CsvEscape(response.Vitals.PulseBpm.ToString()),
                CsvEscape(response.VitalsWarning),
            };

            if (response.Medications.Count == 0)
            {
                csv.AppendLine(string.Join(',', commonFields.Concat(new[] { CsvEscape(null), CsvEscape(null), CsvEscape(null), CsvEscape(null), CsvEscape(null) })));
                continue;
            }

            foreach (var medication in response.Medications)
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

        var fileName = $"patient-{patientId}-history-{DateTime.UtcNow:yyyyMMdd}.csv";
        return (fileName, csv.ToString());
    }

    public async Task<(string FileName, string CsvContent)> ExportListCsvAsync(string? search, CancellationToken cancellationToken)
    {
        var patients = await GetAllAsync(search, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("FullName,DateOfBirth,Age,Gender,PhoneNumber,Address,CreatedAtUtc");

        foreach (var patient in patients)
        {
            var row = new[]
            {
                CsvEscape(patient.FullName),
                CsvEscape(patient.DateOfBirth.ToString("yyyy-MM-dd")),
                CsvEscape(patient.Age.ToString()),
                CsvEscape(patient.Gender),
                CsvEscape(patient.PhoneNumber),
                CsvEscape(patient.Address),
                CsvEscape(patient.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm")),
            };
            csv.AppendLine(string.Join(',', row));
        }

        var fileName = $"patients-{DateTime.UtcNow:yyyyMMdd}.csv";
        return (fileName, csv.ToString());
    }

    public async Task<(string FileName, byte[] Bytes)?> ExportRecordPdfAsync(Guid patientId, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == patientId, cancellationToken);
        if (patient is null)
            return null;

        var patientResponse = ToResponse(patient);
        var visits = await GetVisitsAsync(patientId, null, null, cancellationToken);

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text("Patient Record").FontSize(18).Bold();
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(12).Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Text($"Name: {patientResponse.FullName}").Bold();
                    column.Item().Text($"Date of birth: {patientResponse.DateOfBirth:MMM d, yyyy} ({patientResponse.Age} yrs)");
                    column.Item().Text($"Gender: {patientResponse.Gender}");
                    column.Item().Text($"Phone: {patientResponse.PhoneNumber}");
                    column.Item().Text($"Address: {patientResponse.Address ?? "—"}");
                    column.Item().Text($"Registered on: {patientResponse.CreatedAtUtc:MMM d, yyyy}");

                    column.Item().PaddingTop(10).Text("Visit History").FontSize(13).Bold();

                    if (visits.Count == 0)
                    {
                        column.Item().Text("No visits recorded yet.");
                    }
                    else
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1.5f);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Date").Bold();
                                header.Cell().Text("Diagnosis").Bold();
                                header.Cell().Text("Medications").Bold();
                                header.Cell().Text("Vitals").Bold();
                            });

                            foreach (var visit in visits)
                            {
                                var response = VisitMapper.ToResponse(visit);
                                table.Cell().Text(response.VisitDateUtc.ToString("MMM d, yyyy"));
                                table.Cell().Text(response.Diagnosis);
                                table.Cell().Text(response.Medications.Count > 0
                                    ? string.Join(", ", response.Medications.Select(m => m.Name))
                                    : "—");
                                table.Cell().Text(
                                    $"{response.Vitals.TemperatureCelsius}°C, {response.Vitals.BloodPressureSystolic}/{response.Vitals.BloodPressureDiastolic}, {response.Vitals.PulseBpm}bpm");
                            }
                        });
                    }
                });
            });
        }).GeneratePdf();

        var fileName = $"patient-{patientId}-record.pdf";
        return (fileName, bytes);
    }

    private async Task<List<Visit>> GetVisitsAsync(Guid patientId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var query = _dbContext.Visits
            .AsNoTracking()
            .Include(v => v.Vitals)
            .Include(v => v.Medications)
            .Where(v => v.PatientId == patientId);

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

        return await query.OrderByDescending(v => v.VisitDateUtc).ToListAsync(cancellationToken);
    }

    private static string CsvEscape(string? value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }

    private static PatientResponse ToResponse(Patient patient)
    {
        return new PatientResponse
        {
            Id = patient.Id,
            FullName = patient.FullName,
            DateOfBirth = patient.DateOfBirth,
            Age = CalculateAge(patient.DateOfBirth),
            Gender = patient.Gender,
            PhoneNumber = patient.PhoneNumber,
            Address = patient.Address,
            CreatedAtUtc = patient.CreatedAtUtc,
        };
    }

    private static int CalculateAge(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
            age--;

        return Math.Max(age, 0);
    }
}
