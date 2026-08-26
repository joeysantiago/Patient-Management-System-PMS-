using PatientManagement.Api.Models.Auth;

namespace PatientManagement.Api.Services;

public interface IAuthService
{
    LoginResponse? Login(LoginRequest request);
}
