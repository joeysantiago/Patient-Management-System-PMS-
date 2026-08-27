using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Settings;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class ClinicSettingsService : IClinicSettingsService
{
    private readonly AppDbContext _dbContext;
    private readonly DoctorAccountSettings _doctorAccountSettings;

    public ClinicSettingsService(AppDbContext dbContext, IOptions<DoctorAccountSettings> doctorAccountSettings)
    {
        _dbContext = dbContext;
        _doctorAccountSettings = doctorAccountSettings.Value;
    }

    public async Task<ClinicSettingsResponse> GetAsync(CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateAsync(cancellationToken);
        return ToResponse(settings);
    }

    public async Task<ClinicSettingsResponse> UpdateAsync(ClinicSettingsUpdateRequest request, CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateAsync(cancellationToken);

        settings.ClinicName = request.ClinicName.Trim();
        settings.DoctorName = request.DoctorName.Trim();
        settings.RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber) ? null : request.RegistrationNumber.Trim();
        settings.Qualification = string.IsNullOrWhiteSpace(request.Qualification) ? null : request.Qualification.Trim();
        settings.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
        settings.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        settings.FooterNote = string.IsNullOrWhiteSpace(request.FooterNote) ? null : request.FooterNote.Trim();
        settings.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(settings);
    }

    private async Task<ClinicSettings> GetOrCreateAsync(CancellationToken cancellationToken)
    {
        var settings = await _dbContext.ClinicSettings.FirstOrDefaultAsync(cancellationToken);
        if (settings is not null)
            return settings;

        settings = new ClinicSettings
        {
            Id = Guid.NewGuid(),
            ClinicName = "My Clinic",
            DoctorName = _doctorAccountSettings.DisplayName,
            UpdatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.ClinicSettings.Add(settings);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return settings;
    }

    private static ClinicSettingsResponse ToResponse(ClinicSettings settings)
    {
        return new ClinicSettingsResponse
        {
            ClinicName = settings.ClinicName,
            DoctorName = settings.DoctorName,
            RegistrationNumber = settings.RegistrationNumber,
            Qualification = settings.Qualification,
            Phone = settings.Phone,
            Address = settings.Address,
            FooterNote = settings.FooterNote,
            UpdatedAtUtc = settings.UpdatedAtUtc,
        };
    }
}
