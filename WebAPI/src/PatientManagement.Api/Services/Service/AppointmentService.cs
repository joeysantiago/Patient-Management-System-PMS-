using System.Text;
using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Appointments;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class AppointmentService : IAppointmentService
{
    // Every appointment needs at least a full 15-minute gap from any other Scheduled
    // appointment; a slot exactly 15 minutes away (e.g. 6:10 and 6:25) still counts as
    // taken, so the next bookable time is 16 minutes out (6:26).
    public const int AppointmentDurationMinutes = 15;

    private readonly AppDbContext _dbContext;

    public AppointmentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetAllAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var query = _dbContext.Appointments.AsNoTracking().Include(a => a.Patient).AsQueryable();

        if (date.HasValue)
        {
            var startOfDay = date.Value.ToDateTime(TimeOnly.MinValue);
            var startOfNextDay = startOfDay.AddDays(1);
            query = query.Where(a => a.ScheduledAt >= startOfDay && a.ScheduledAt < startOfNextDay);
        }

        var appointments = await query
            .OrderBy(a => a.ScheduledAt)
            .ToListAsync(cancellationToken);

        return appointments.Select(ToResponse).ToList();
    }

    public async Task<AppointmentResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        return appointment is null ? null : ToResponse(appointment);
    }

    public async Task<AppointmentCreateResult> CreateAsync(AppointmentCreateRequest request, CancellationToken cancellationToken)
    {
        var patient = await _dbContext.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken);
        if (patient is null)
            return new AppointmentCreateResult { Status = AppointmentCreateStatus.PatientNotFound };

        var timeSlotTaken = await IsTimeSlotTakenAsync(request.ScheduledAt, excludeAppointmentId: null, cancellationToken);
        if (timeSlotTaken)
            return new AppointmentCreateResult { Status = AppointmentCreateStatus.TimeSlotAlreadyBooked };

        var requestedDate = DateOnly.FromDateTime(request.ScheduledAt);
        var startOfDay = requestedDate.ToDateTime(TimeOnly.MinValue);
        var startOfNextDay = startOfDay.AddDays(1);
        var patientAlreadyScheduledThatDay = await _dbContext.Appointments.AnyAsync(
            a => a.PatientId == request.PatientId
                && a.Status == AppointmentStatus.Scheduled
                && a.ScheduledAt >= startOfDay
                && a.ScheduledAt < startOfNextDay,
            cancellationToken);
        if (patientAlreadyScheduledThatDay)
            return new AppointmentCreateResult { Status = AppointmentCreateStatus.PatientAlreadyScheduledOnDate };

        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = request.PatientId,
            ScheduledAt = request.ScheduledAt,
            Status = AppointmentStatus.Scheduled,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        appointment.Patient = patient;
        return new AppointmentCreateResult { Status = AppointmentCreateStatus.Success, Appointment = ToResponse(appointment) };
    }

    public async Task<AppointmentUpdateResult> UpdateAsync(Guid id, AppointmentUpdateRequest request, CancellationToken cancellationToken)
    {
        var appointment = await _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (appointment is null)
            return AppointmentUpdateResult.AppointmentNotFound;

        var patientExists = await _dbContext.Patients.AnyAsync(p => p.Id == request.PatientId, cancellationToken);
        if (!patientExists)
            return AppointmentUpdateResult.PatientNotFound;

        if (request.Status == AppointmentStatus.Scheduled)
        {
            var timeSlotTaken = await IsTimeSlotTakenAsync(request.ScheduledAt, excludeAppointmentId: id, cancellationToken);
            if (timeSlotTaken)
                return AppointmentUpdateResult.TimeSlotAlreadyBooked;
        }

        appointment.PatientId = request.PatientId;
        appointment.ScheduledAt = request.ScheduledAt;
        appointment.Status = request.Status;
        appointment.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return AppointmentUpdateResult.Success;
    }

    public async Task<(string FileName, string CsvContent)> ExportCsvAsync(DateOnly? date, CancellationToken cancellationToken)
    {
        var appointments = await GetAllAsync(date, cancellationToken);

        var csv = new StringBuilder();
        csv.AppendLine("PatientName,PatientPhoneNumber,ScheduledAt,Status,Notes");

        foreach (var appointment in appointments)
        {
            var row = new[]
            {
                CsvEscape(appointment.PatientName),
                CsvEscape(appointment.PatientPhoneNumber),
                CsvEscape(appointment.ScheduledAt.ToString("yyyy-MM-dd HH:mm")),
                CsvEscape(appointment.Status.ToString()),
                CsvEscape(appointment.Notes),
            };
            csv.AppendLine(string.Join(',', row));
        }

        var suffix = date.HasValue ? date.Value.ToString("yyyyMMdd") : "all";
        var fileName = $"appointments-{suffix}.csv";
        return (fileName, csv.ToString());
    }

    private async Task<bool> IsTimeSlotTakenAsync(DateTime scheduledAt, Guid? excludeAppointmentId, CancellationToken cancellationToken)
    {
        var lowerBound = scheduledAt.AddMinutes(-AppointmentDurationMinutes);
        var upperBound = scheduledAt.AddMinutes(AppointmentDurationMinutes);

        return await _dbContext.Appointments.AnyAsync(
            a => a.Status == AppointmentStatus.Scheduled
                && a.Id != excludeAppointmentId
                && a.ScheduledAt >= lowerBound && a.ScheduledAt <= upperBound,
            cancellationToken);
    }

    private static string CsvEscape(string? value)
    {
        return $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
    }

    private static AppointmentResponse ToResponse(Appointment appointment)
    {
        return new AppointmentResponse
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            PatientName = appointment.Patient?.FullName ?? string.Empty,
            PatientPhoneNumber = appointment.Patient?.PhoneNumber ?? string.Empty,
            ScheduledAt = appointment.ScheduledAt,
            Status = appointment.Status,
            Notes = appointment.Notes,
            CreatedAtUtc = appointment.CreatedAtUtc,
        };
    }
}
