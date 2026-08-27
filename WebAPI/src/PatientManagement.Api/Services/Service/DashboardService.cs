using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Models.Dashboard;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class DashboardService : IDashboardService
{
    private const int RecentPatientsCount = 5;

    private readonly AppDbContext _dbContext;
    private readonly IAppointmentService _appointmentService;
    private readonly IPatientService _patientService;

    public DashboardService(AppDbContext dbContext, IAppointmentService appointmentService, IPatientService patientService)
    {
        _dbContext = dbContext;
        _appointmentService = appointmentService;
        _patientService = patientService;
    }

    public async Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var newPatientsLast30Days = await _dbContext.Patients
            .AsNoTracking()
            .CountAsync(p => p.CreatedAtUtc >= thirtyDaysAgo, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaysAppointments = await _appointmentService.GetAllAsync(today, cancellationToken);

        var recentPatients = await _patientService.GetRecentAsync(RecentPatientsCount, cancellationToken);

        return new DashboardSummaryResponse
        {
            NewPatientsLast30Days = newPatientsLast30Days,
            TodaysAppointmentCount = todaysAppointments.Count,
            TodaysAppointments = todaysAppointments.ToList(),
            RecentPatients = recentPatients.ToList(),
        };
    }
}
