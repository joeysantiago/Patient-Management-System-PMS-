using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Consultations;

namespace PatientManagement.Api.Services.Service;

internal static class VisitMapper
{
    private const int SystolicLow = 90;
    private const int SystolicHigh = 140;
    private const int DiastolicLow = 60;
    private const int DiastolicHigh = 90;

    public static VisitResponse ToResponse(Visit visit)
    {
        var vitals = visit.Vitals;
        var outOfRange = false;
        string? warning = null;
        if (vitals is not null)
            (outOfRange, warning) = EvaluateVitals(vitals);

        return new VisitResponse
        {
            Id = visit.Id,
            PatientId = visit.PatientId,
            AppointmentId = visit.AppointmentId,
            VisitDateUtc = visit.VisitDateUtc,
            Vitals = vitals is null
                ? new VitalsDto()
                : new VitalsDto
                {
                    TemperatureCelsius = vitals.TemperatureCelsius,
                    BloodPressureSystolic = vitals.BloodPressureSystolic,
                    BloodPressureDiastolic = vitals.BloodPressureDiastolic,
                    PulseBpm = vitals.PulseBpm,
                },
            Complaints = visit.Complaints,
            Diagnosis = visit.Diagnosis,
            Medications = visit.Medications.Select(m => new MedicationDto
            {
                Name = m.Name,
                Dosage = m.Dosage,
                Frequency = m.Frequency,
                Duration = m.Duration,
                Instructions = m.Instructions,
            }).ToList(),
            VitalsOutOfRange = outOfRange,
            VitalsWarning = warning,
            CreatedAtUtc = visit.CreatedAtUtc,
            UpdatedAtUtc = visit.UpdatedAtUtc,
        };
    }

    // Basic non-blocking hint only, not a diagnostic tool. Thresholds are approximate typical adult ranges.
    private static (bool OutOfRange, string? Warning) EvaluateVitals(Vitals vitals)
    {
        var systolicOutOfRange = vitals.BloodPressureSystolic < SystolicLow || vitals.BloodPressureSystolic > SystolicHigh;
        var diastolicOutOfRange = vitals.BloodPressureDiastolic < DiastolicLow || vitals.BloodPressureDiastolic > DiastolicHigh;

        if (systolicOutOfRange || diastolicOutOfRange)
            return (true, $"Blood pressure is outside the typical {SystolicLow}-{SystolicHigh}/{DiastolicLow}-{DiastolicHigh} mmHg range. Please double-check the reading.");

        return (false, null);
    }
}
