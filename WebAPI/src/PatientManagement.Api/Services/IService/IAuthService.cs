using PatientManagement.Api.Models.Auth;

namespace PatientManagement.Api.Services.IService;

public enum ChangePasswordResult
{
    Success,
    CurrentPasswordIncorrect,
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<ChangePasswordResult> ChangePasswordAsync(string username, ChangePasswordRequest request, CancellationToken cancellationToken);
}
