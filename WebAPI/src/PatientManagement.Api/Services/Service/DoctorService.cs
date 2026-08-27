using Microsoft.EntityFrameworkCore;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Models.Doctors;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class DoctorService : IDoctorService
{
    private readonly AppDbContext _dbContext;

    public DoctorService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DoctorResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var doctors = await _dbContext.Doctors
            .AsNoTracking()
            .OrderBy(d => d.FullName)
            .ToListAsync(cancellationToken);

        return doctors.Select(ToResponse).ToList();
    }

    public async Task<DoctorResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return doctor is null ? null : ToResponse(doctor);
    }

    public async Task<DoctorResponse> CreateAsync(DoctorCreateRequest request, CancellationToken cancellationToken)
    {
        var doctor = new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName.Trim(),
            Specialization = request.Specialization.Trim(),
            RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber) ? null : request.RegistrationNumber.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            ClinicAddress = string.IsNullOrWhiteSpace(request.ClinicAddress) ? null : request.ClinicAddress.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
        };

        _dbContext.Doctors.Add(doctor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(doctor);
    }

    public async Task<DoctorResponse?> UpdateAsync(Guid id, DoctorUpdateRequest request, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor is null)
            return null;

        doctor.FullName = request.FullName.Trim();
        doctor.Specialization = request.Specialization.Trim();
        doctor.RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber) ? null : request.RegistrationNumber.Trim();
        doctor.PhoneNumber = request.PhoneNumber.Trim();
        doctor.Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        doctor.ClinicAddress = string.IsNullOrWhiteSpace(request.ClinicAddress) ? null : request.ClinicAddress.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(doctor);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        if (doctor is null)
            return false;

        _dbContext.Doctors.Remove(doctor);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static DoctorResponse ToResponse(Doctor doctor)
    {
        return new DoctorResponse
        {
            Id = doctor.Id,
            FullName = doctor.FullName,
            Specialization = doctor.Specialization,
            RegistrationNumber = doctor.RegistrationNumber,
            PhoneNumber = doctor.PhoneNumber,
            Email = doctor.Email,
            ClinicAddress = doctor.ClinicAddress,
            CreatedAtUtc = doctor.CreatedAtUtc,
        };
    }
}
