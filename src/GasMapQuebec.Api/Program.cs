using System.Text.Encodings.Web;
using Microsoft.AspNetCore.HttpOverrides;
using GasMapQuebec.Api;
using GasMapQuebec.FuelLog.Infrastructure;
using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Extensions;
using Hangfire.PostgreSql;
using GasMapQuebec.Pricing.Application;
using GasMapQuebec.Pricing.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers()
    // Keep accented text (e.g. "Région") literal in v1 JSON responses; camelCase is the default.
    .AddJsonOptions(o => o.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping);

// Transport compression is handled by the Caddy ingress (encode zstd gzip), which must be
// configured to SKIP /stations.geojson — that endpoint serves pre-gzipped payload bytes with no
// Content-Encoding header, so compressing it again would double-gzip and break the mobile client.

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Modules
builder.AddPricingModule();
builder.AddFuelLogModule();

// Hangfire — recurring background jobs, stored in the shared PostgreSQL database.
var hangfireConnectionString = builder.Configuration.GetConnectionString(PricingModule.DatabaseConnectionName)
    ?? throw new InvalidOperationException(
        $"Connection string '{PricingModule.DatabaseConnectionName}' was not found.");

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(
        options => options.UseNpgsqlConnection(hangfireConnectionString),
        new PostgreSqlStorageOptions { SchemaName = "hangfire" })
    // Surface per-job logs (and progress) on the dashboard's job details page.
    .UseConsole());
// Routes ILogger output from the running job into the Hangfire Console, so the
// existing logging in PriceRefreshService shows up in the dashboard.
builder.Services.AddHangfireConsoleExtensions();
builder.Services.AddHangfireServer();

var app = builder.Build();

// Behind the Caddy ingress (TLS terminated at the edge, plain HTTP forwarded to the container):
// trust X-Forwarded-Proto/For so Request.Scheme and UseHttpsRedirection behave correctly. The
// only thing that can reach the container's 127.0.0.1-bound port is Caddy, so all proxies are trusted.
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeaders.KnownIPNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

app.MapDefaultEndpoints();

// Apply EF migrations on startup in Development, or in any environment when explicitly opted in
// (RunMigrationsAtStartup=true) — the controlled step for the single-instance docker-compose deploy.
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("RunMigrationsAtStartup"))
{
    await app.ApplyMigrationsAsync();
}

// Dev-only tooling (OpenAPI/Scalar, and the unauthenticated Hangfire dashboard).
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Pull the latest prices every 10 minutes.
app.Services.GetRequiredService<IRecurringJobManager>().AddOrUpdate<IPriceRefreshService>(
    "pricing:refresh-prices",
    service => service.RefreshAsync(CancellationToken.None),
    "*/10 * * * *");

app.Run();
