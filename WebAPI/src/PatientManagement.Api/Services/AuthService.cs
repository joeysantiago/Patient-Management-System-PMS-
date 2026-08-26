using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Models.Auth;

namespace PatientManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly DoctorAccountSettings _doctorAccount;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IOptions<DoctorAccountSettings> doctorAccount, IOptions<JwtSettings> jwtSettings)
    {
        _doctorAccount = doctorAccount.Value;
        _jwtSettings = jwtSettings.Value;
    }

    public LoginResponse? Login(LoginRequest request)
    {
        if (!string.Equals(request.Username, _doctorAccount.Username, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, _doctorAccount.PasswordHash))
        {
            return null;
        }

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var token = GenerateToken(_doctorAccount.Username, expiresAtUtc);

        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            DisplayName = _doctorAccount.DisplayName,
            Username = _doctorAccount.Username,
        };
    }

    private string GenerateToken(string username, DateTime expiresAtUtc)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, username),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
