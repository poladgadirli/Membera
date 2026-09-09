using Membera.Merchant.Api.Middleware;
using Membera.Merchant.Application.Abstractions;
using Membera.Merchant.Application.Merchants.CreateMerchant;
using Membera.Merchant.Application.Merchants.GetMerchantByOwnerId;
using Membera.Merchant.Application.Merchants.UpdateMerchant;
using Membera.Merchant.Application.SubscriptionPlans.CreateSubscriptionPlan;
using Membera.Merchant.Application.SubscriptionPlans.DeactivateSubscriptionPlan;
using Membera.Merchant.Application.SubscriptionPlans.GetPlansByMerchantId;
using Membera.Merchant.Application.SubscriptionPlans.UpdateSubscriptionPlan;
using Membera.Merchant.Application.Subscriptions.CreateCheckoutSession;
using Membera.Merchant.Application.Subscriptions.GetMySubscriptions;
using Membera.Merchant.Application.Subscriptions.HandleStripeWebhook;
using Membera.Merchant.Application.Subscriptions.RedeemSubscription;
using Membera.Merchant.Infrastructure.Messaging;
using Membera.Merchant.Infrastructure.Payments;
using Membera.Merchant.Infrastructure.Persistence;
using Membera.Shared.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

// Load environment variables from the repo-root .env file (the same file
// docker-compose.yml uses) so JWT_SECRET_KEY / POSTGRES_PASSWORD are picked up
// without manually setting OS-level environment variables on every machine.
LoadDotEnv();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/merchant-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// DbContext qeydiyyatı
var postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
var connectionString = postgresPassword is not null
    ? $"Host=localhost;Port=5432;Database=membera_merchant;Username=postgres;Password={postgresPassword}"
    : builder.Configuration.GetConnectionString("MerchantDb");

builder.Services.AddDbContext<MerchantDbContext>(options =>
    options.UseNpgsql(connectionString));

// Repository
builder.Services.AddScoped<IMerchantRepository, MerchantRepository>();

// Handler-lər
builder.Services.AddScoped<CreateMerchantHandler>();
builder.Services.AddScoped<GetMerchantByOwnerIdHandler>();
builder.Services.AddScoped<UpdateMerchantHandler>();
builder.Services.AddHostedService<UserRegisteredConsumer>();

builder.Services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();

builder.Services.AddScoped<CreateSubscriptionPlanHandler>();
builder.Services.AddScoped<GetPlansByMerchantIdHandler>();
builder.Services.AddScoped<UpdateSubscriptionPlanHandler>();
builder.Services.AddScoped<DeactivateSubscriptionPlanHandler>();

builder.Services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
builder.Services.AddScoped<IStripeService, StripeService>();

builder.Services.AddScoped<CreateCheckoutSessionHandler>();
builder.Services.AddScoped<HandleStripeWebhookHandler>();
builder.Services.AddScoped<RedeemSubscriptionHandler>();
builder.Services.AddScoped<GetMySubscriptionsHandler>();

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));

builder.Services.AddScoped<ICacheService, RedisCacheService>();

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
        Description = "Enter the JWT access token (no need to type 'Bearer', it's added automatically)"
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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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