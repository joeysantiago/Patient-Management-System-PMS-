using PatientManagement.Api.Models.Dashboard;

namespace PatientManagement.Api.Services.IService;

public interface IDashboardService
{
    Task<DashboardSummaryResponse> GetSummaryAsync(CancellationToken cancellationToken);
}
