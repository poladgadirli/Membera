var builder = WebApplication.CreateBuilder(args);

// The cluster destinations in appsettings.json point at the services' local
// dev (https://localhost:72xx) ports. Inside docker-compose the gateway has to
// reach the other containers by their compose service name and internal port
// (8080) instead, so these env vars - set in docker-compose.yml - override the
// destination addresses. Left unset, the hardcoded appsettings.json values are
// used, preserving normal local (non-Docker) dev.
var authServiceUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL");
if (authServiceUrl is not null)
{
    builder.Configuration["ReverseProxy:Clusters:auth-cluster:Destinations:auth-destination:Address"] = authServiceUrl;
}

var merchantServiceUrl = Environment.GetEnvironmentVariable("MERCHANT_SERVICE_URL");
if (merchantServiceUrl is not null)
{
    builder.Configuration["ReverseProxy:Clusters:merchant-cluster:Destinations:merchant-destination:Address"] = merchantServiceUrl;
}

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// CORS – the SPA now talks to the gateway instead of the individual services, so
// the browser's cross-origin checks (and the OPTIONS preflight that PUT/POST with
// an Authorization header trigger) have to be answered here. Kept identical to
// Membera.Auth.Api / Membera.Merchant.Api so behaviour is the same wherever the
// frontend points.
const string frontendCorsPolicy = "FrontendCors";
var frontendOrigins = (Environment.GetEnvironmentVariable("FRONTEND_ORIGINS")
                       ?? "http://localhost:5173;http://localhost:5174")
    .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
        policy.WithOrigins(frontendOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// CORS must run before MapReverseProxy so the CORS middleware can answer the
// browser's preflight itself (204 + headers) instead of that OPTIONS request
// being proxied upstream.
app.UseCors(frontendCorsPolicy);

app.MapReverseProxy();

app.Run();
