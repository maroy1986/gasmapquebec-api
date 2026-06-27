using API;
using FuelLog.Infrastructure;
using Hangfire;
using Hangfire.PostgreSql;
using Pricing.Application;
using Pricing.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();

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
        new PostgreSqlStorageOptions { SchemaName = "hangfire" }));
builder.Services.AddHangfireServer();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    await app.ApplyMigrationsAsync();
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
