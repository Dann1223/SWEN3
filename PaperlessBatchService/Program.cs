using Microsoft.EntityFrameworkCore;
using PaperlessBatchService.Data;
using PaperlessBatchService.Services;
using PaperlessBatchService.Configuration;
using PaperlessBatchService.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/batch-service-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddSerilog();

// Add configuration
builder.Services.Configure<BatchProcessingOptions>(
    builder.Configuration.GetSection(BatchProcessingOptions.SectionName));

// Add database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BatchDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services
builder.Services.AddScoped<IBatchProcessingService, BatchProcessingService>();

// Add hosted service
builder.Services.AddHostedService<BatchProcessingWorker>();

var host = builder.Build();

try
{
    Log.Information("Starting Paperless Batch Service");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Paperless Batch Service terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
