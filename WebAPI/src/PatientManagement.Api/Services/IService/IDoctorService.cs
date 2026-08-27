using PatientManagement.Api.Models.Doctors;

namespace PatientManagement.Api.Services.IService;

public interface IDoctorService
{
    Task<IReadOnlyList<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<DoctorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<DoctorResponse> CreateAsync(DoctorCreateRequest request, CancellationToken cancellationToken);

    Task<DoctorResponse?> UpdateAsync(Guid id, DoctorUpdateRequest request, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
