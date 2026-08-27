using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PatientManagement.Api.Configuration;
using PatientManagement.Api.Data;
using PatientManagement.Api.Data.Entities;
using PatientManagement.Api.Services.IService;
using PatientManagement.Api.Services.Service;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Microsoft.EntityFrameworkCore.Sqlite transitively references the plain SQLitePCLRaw.bundle_e_sqlite3
// package regardless of the SQLCipher bundle also referenced above, so both native providers are present
// in the output. Without forcing the provider here, whichever one's module initializer runs first would
// win non-deterministically — silently disabling encryption. This must run before any SQLite connection opens.
// Note: the managed namespace is "SQLitePCL" (no trailing "Raw") even though the NuGet packages are named
// "SQLitePCLRaw.*" — the "Raw" suffix is part of the package/repo name only. bundle_e_sqlcipher (not the
// older, unmaintained bundle_sqlcipher) is used because it's version-locked with bundle_e_sqlite3, so its
// ISQLite3Provider implementation is always ABI-compatible with whatever Microsoft.Data.Sqlite expects.
SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_e_sqlcipher());

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.Configure<DoctorAccountSettings>(builder.Configuration.GetSection(DoctorAccountSettings.SectionName));
builder.Services.Configure<BackupSettings>(builder.Configuration.GetSection(BackupSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is missing.");

var corsAllowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPatientService, PatientService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IConsultationService, ConsultationService>();
builder.Services.AddScoped<IDoctorService, DoctorService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IClinicSettingsService, ClinicSettingsService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddHostedService<DatabaseBackupHostedService>();

var sqliteEncryptionKey = builder.Configuration["Sqlite:EncryptionKey"]
    ?? throw new InvalidOperationException("Sqlite:EncryptionKey configuration is missing.");
var baseConnectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Default connection string is missing.");
var encryptedConnectionString = new SqliteConnectionStringBuilder(baseConnectionString)
{
    Password = sqliteEncryptionKey,
}.ToString();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(encryptedConnectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("WebUi", policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
        };
    });

builder.Services.AddAuthorization();

// Basic brute-force protection on the login endpoint.
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();

    // One-time seed: the doctor account used to live only in appsettings.json. Once a row
    // exists in the database it becomes the source of truth (e.g. after a password change).
    if (!dbContext.DoctorAccounts.Any())
    {
        var doctorAccountSettings = scope.ServiceProvider.GetRequiredService<IOptions<DoctorAccountSettings>>().Value;
        dbContext.DoctorAccounts.Add(new DoctorAccount
        {
            Id = Guid.NewGuid(),
            Username = doctorAccountSettings.Username,
            PasswordHash = doctorAccountSettings.PasswordHash,
            DisplayName = doctorAccountSettings.DisplayName,
            UpdatedAtUtc = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

// Skipped in Development: the Angular dev server talks to the API over plain HTTP
// (see environment.development.ts), and redirecting to HTTPS here would bounce the
// CORS preflight (OPTIONS) request through a 307 — which the CORS spec treats as a
// hard failure, surfacing as a browser CORS error rather than a redirect.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("WebUi");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
