using FluentValidation;
using FluentValidation.AspNetCore;
using Membera.Auth.Api.Middleware;
using Membera.Auth.Application.Abstractions;
using Membera.Auth.Application.Auth.Register;
using Membera.Auth.Infrastructure.Persistence;
using Membera.Auth.Infrastructure.Security;
using Membera.Shared.Messaging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

// Load environment variables from the repo-root .env file (the same file
// docker-compose.yml uses) so JWT_SECRET_KEY / POSTGRES_PASSWORD are picked up
// without manually setting OS-level environment variables on every machine.
LoadDotEnv();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// DbContext qeydiyyatı
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var connectionString = postgresPassword is not null
    ? $"Host=localhost;Port=5432;Database=membera_auth;Username=postgres;Password={postgresPassword}"
    : builder.Configuration.GetConnectionString("AuthDb");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository və Servislər
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Login.LoginHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.RefreshAccessToken.RefreshAccessTokenHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.ChangePassword.ChangePasswordHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Logout.LogoutHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.ChangeEmail.ChangeEmailHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.DeleteAccount.DeleteAccountHandler>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.GoogleLogin.GoogleLoginHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Admin.GetAllUsers.GetAllUsersHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Admin.DeleteUserByAdmin.DeleteUserByAdminHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Admin.PromoteToAdmin.PromoteToAdminHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Admin.DemoteAdmin.DemoteAdminHandler>();
builder.Services.AddScoped<Membera.Auth.Application.Auth.Admin.DeleteAdminAccount.DeleteAdminAccountHandler>();

var rabbitMqPublisher = await RabbitMqEventPublisher.CreateAsync("localhost");
builder.Services.AddSingleton<IEventPublisher>(rabbitMqPublisher);

// JWT Authentication
var jwtSection = builder.Configuration.GetSection("Jwt");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? jwtSection["SecretKey"]!;
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? jwtSection["Issuer"];
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? jwtSection["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT access token-i daxil et (Bearer sözünü yazmağa ehtiyac yoxdur, avtomatik əlavə olunur)"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Walks upward from the current working directory and the app base directory
// (e.g. bin/Debug/net8.0 when launched from Visual Studio) until it finds a
// .env file, then loads it. The .env lives at the repo root, several levels
// above each Api project's output directory, so a search-upward approach works
// no matter which service runs or from where. If no .env is found the app keeps
// running on OS environment variables / appsettings (the existing fallback).
static void LoadDotEnv()
{
    foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
    {
        for (var dir = new DirectoryInfo(start); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, ".env");
            if (File.Exists(candidate))
            {
                // NoClobber: an already-set OS environment variable wins over .env.
                DotNetEnv.Env.NoClobber().Load(candidate);
                Console.WriteLine($"[env] Loaded environment variables from {candidate}");
                return;
            }
        }
    }

    Console.WriteLine("[env] Warning: no .env file found in any parent directory; " +
        "falling back to OS environment variables and appsettings.json.");
}