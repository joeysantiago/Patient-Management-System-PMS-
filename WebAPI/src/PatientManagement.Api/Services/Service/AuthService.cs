using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Data;
using PatientManagement.Api.Models.Auth;
using PatientManagement.Api.Services.IService;

namespace PatientManagement.Api.Services.Service;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(AppDbContext dbContext, IOptions<JwtSettings> jwtSettings)
    {
        _dbContext = dbContext;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var account = await _dbContext.DoctorAccounts.AsNoTracking().FirstOrDefaultAsync(cancellationToken);
        if (account is null || !string.Equals(request.Username, account.Username, StringComparison.OrdinalIgnoreCase))
            return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
            return null;

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);
        var token = GenerateToken(account.Username, expiresAtUtc);

        return new LoginResponse
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            DisplayName = account.DisplayName,
            Username = account.Username,
        };
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(string username, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var account = await _dbContext.DoctorAccounts.FirstOrDefaultAsync(a => a.Username == username, cancellationToken);
        if (account is null || !BCrypt.Net.BCrypt.Verify(request.CurrentPassword, account.PasswordHash))
            return ChangePasswordResult.CurrentPasswordIncorrect;

        account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        account.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChangePasswordResult.Success;
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
