using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PatientManagement.Api.Models.Auth;
using PatientManagement.Api.Services;

namespace PatientManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("login")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        var result = _authService.Login(request);

        if (result is null)
        {
            _logger.LogWarning("Failed login attempt for username {Username}", request.Username);
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(result);
    }
}
