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

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/auth-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// DbContext qeydiyyatı
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AuthDb")));

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
var secretKey = jwtSection["SecretKey"]!;

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
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
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