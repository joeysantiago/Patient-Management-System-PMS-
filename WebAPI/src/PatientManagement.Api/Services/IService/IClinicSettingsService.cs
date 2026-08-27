using PatientManagement.Api.Models.Settings;

namespace PatientManagement.Api.Services.IService;

public interface IClinicSettingsService
{
    Task<ClinicSettingsResponse> GetAsync(CancellationToken cancellationToken);

    Task<ClinicSettingsResponse> UpdateAsync(ClinicSettingsUpdateRequest request, CancellationToken cancellationToken);
}
