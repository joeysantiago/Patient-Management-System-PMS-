using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Consultations;
using PatientManagement.Api.Services.IService;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PatientManagement.Api.Services.Service;

public class ConsultationService : IConsultationService
{
    private readonly AppDbContext _dbContext;
    private readonly IClinicSettingsService _clinicSettingsService;
    private readonly IPatientService _patientService;

    public ConsultationService(AppDbContext dbContext, IClinicSettingsService clinicSettingsService, IPatientService patientService)
    {
        _dbContext = dbContext;
        _clinicSettingsService = clinicSettingsService;
        _patientService = patientService;
    }

    public async Task<VisitResponse?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var visit = await _dbContext.Visits
            .AsNoTracking()
            .Include(v => v.Vitals)
            .Include(v => v.Medications)
            .FirstOrDefaultAsync(v => v.AppointmentId == appointmentId, cancellationToken);

        return visit is null ? null : VisitMapper.ToResponse(visit);
    }

    public async Task<PrescriptionResponse?> GetPrescriptionAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var visit = await GetByAppointmentIdAsync(appointmentId, cancellationToken);
        if (visit is null)
            return null;

        var patient = await _patientService.GetByIdAsync(visit.PatientId, cancellationToken);
        if (patient is null)
            return null;

        var clinic = await _clinicSettingsService.GetAsync(cancellationToken);

        return new PrescriptionResponse
        {
            Clinic = clinic,
            Patient = patient,
            Visit = visit,
        };
    }

    public async Task<(string FileName, byte[] Bytes)?> ExportPrescriptionPdfAsync(Guid appointmentId, CancellationToken cancellationToken)
    {
        var prescription = await GetPrescriptionAsync(appointmentId, cancellationToken);
        if (prescription is null)
            return null;

        var clinic = prescription.Clinic;
        var patient = prescription.Patient;
        var visit = prescription.Visit;

        var bytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontSize(11));

                page.Header().Column(column =>
                {
                    column.Item().Text(clinic.ClinicName).FontSize(18).Bold();
                    column.Item().Text(text =>
                    {
                        text.Span(clinic.DoctorName).Bold();
                        if (!string.IsNullOrWhiteSpace(clinic.Qualification))
                            text.Span($"  ·  {clinic.Qualification}");
                        if (!string.IsNullOrWhiteSpace(clinic.RegistrationNumber))
                            text.Span($"  ·  Reg. No. {clinic.RegistrationNumber}");
                    });
                    if (!string.IsNullOrWhiteSpace(clinic.Address) || !string.IsNullOrWhiteSpace(clinic.Phone))
                    {
                        var contactLine = string.Join("  ·  ", new[] { clinic.Address, clinic.Phone }.Where(s => !string.IsNullOrWhiteSpace(s)));
                        column.Item().Text(contactLine).FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                    column.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingVertical(12).Column(column =>
                {
                    column.Spacing(8);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text($"Patient: {patient.FullName} ({patient.Age} yrs, {patient.Gender})");
                        row.RelativeItem().AlignRight().Text($"Phone: {patient.PhoneNumber}");
                    });
                    column.Item().Text($"Visit date: {visit.VisitDateUtc:MMM d, yyyy h:mm tt}");

                    column.Item().Text(
                        $"Temp: {visit.Vitals.TemperatureCelsius}°C    BP: {visit.Vitals.BloodPressureSystolic}/{visit.Vitals.BloodPressureDiastolic} mmHg    Pulse: {visit.Vitals.PulseBpm} bpm");

                    column.Item().Text("Complaints").Bold();
                    column.Item().Text(visit.Complaints);

                    column.Item().Text("Diagnosis").Bold();
                    column.Item().Text(visit.Diagnosis);

                    column.Item().Text("Rx — Medications").Bold();
                    if (visit.Medications.Count > 0)
                    {
                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Name").Bold();
                                header.Cell().Text("Dosage").Bold();
                                header.Cell().Text("Frequency").Bold();
                                header.Cell().Text("Duration").Bold();
                                header.Cell().Text("Instructions").Bold();
                            });

                            foreach (var medication in visit.Medications)
                            {
                                table.Cell().Text(medication.Name);
                                table.Cell().Text(medication.Dosage);
                                table.Cell().Text(medication.Frequency);
                                table.Cell().Text(medication.Duration);
                                table.Cell().Text(medication.Instructions ?? "—");
                            }
                        });
                    }
                    else
                    {
                        column.Item().Text("No medications prescribed.");
                    }
                });

                page.Footer().AlignRight().Text(clinic.FooterNote ?? "Signature").FontSize(9).FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();

        var fileName = $"prescription-{appointmentId}.pdf";
        return (fileName, bytes);
    }

    public async Task<ConsultationSaveResult> CreateOrUpdateVisitAsync(VisitCreateRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);
        if (appointment is null)
            return new ConsultationSaveResult { Status = ConsultationSaveStatus.AppointmentNotFound };

        if (appointment.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow)
            return new ConsultationSaveResult { Status = ConsultationSaveStatus.AppointmentCancelledOrNoShow };

        var existingVisit = await _dbContext.Visits
            .Include(v => v.Vitals)
            .Include(v => v.Medications)
            .FirstOrDefaultAsync(v => v.AppointmentId == request.AppointmentId, cancellationToken);

        var now = DateTime.UtcNow;
        Visit visit;

        if (existingVisit is null)
        {
            visit = new Visit
            {
                Id = Guid.NewGuid(),
                PatientId = appointment.PatientId,
                AppointmentId = appointment.Id,
                VisitDateUtc = now,
                CreatedAtUtc = now,
            };
            _dbContext.Visits.Add(visit);

            visit.Vitals = new Vitals { VisitId = visit.Id };

            appointment.Status = AppointmentStatus.Completed;
        }
        else
        {
            visit = existingVisit;
            visit.UpdatedAtUtc = now;

            visit.Vitals ??= new Vitals { VisitId = visit.Id };

            // Medication.VisitId is a required FK, so clearing the collection alone is enough —
            // EF Core marks the now-orphaned dependents as deleted at SaveChanges. Also calling
            // Medications.RemoveRange(...) here double-deletes the same rows and throws a
            // DbUpdateConcurrencyException (0 rows affected on the second delete).
            visit.Medications.Clear();
        }

        visit.Complaints = request.Complaints.Trim();
        visit.Diagnosis = request.Diagnosis.Trim();

        visit.Vitals.TemperatureCelsius = request.Vitals.TemperatureCelsius;
        visit.Vitals.BloodPressureSystolic = request.Vitals.BloodPressureSystolic;
        visit.Vitals.BloodPressureDiastolic = request.Vitals.BloodPressureDiastolic;
        visit.Vitals.PulseBpm = request.Vitals.PulseBpm;

        foreach (var medication in request.Medications)
        {
            visit.Medications.Add(new Medication
            {
                Id = Guid.NewGuid(),
                VisitId = visit.Id,
                Name = medication.Name.Trim(),
                Dosage = medication.Dosage.Trim(),
                Frequency = medication.Frequency.Trim(),
                Duration = medication.Duration.Trim(),
                Instructions = string.IsNullOrWhiteSpace(medication.Instructions) ? null : medication.Instructions.Trim(),
            });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ConsultationSaveResult { Status = ConsultationSaveStatus.Success, Visit = VisitMapper.ToResponse(visit) };
    }
}
