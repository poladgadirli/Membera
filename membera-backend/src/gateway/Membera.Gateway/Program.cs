var builder = WebApplication.CreateBuilder(args);

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
