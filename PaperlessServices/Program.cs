using PaperlessServices.Workers;
using PaperlessServices.Services.Interfaces;
using PaperlessServices.Services.Implementations;
using PaperlessServices.Configuration;
using Minio;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/paperless-services-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = Host.CreateApplicationBuilder(args);

// Add Serilog
builder.Services.AddSerilog();

// Configure MinIO
builder.Services.Configure<MinIOConfig>(builder.Configuration.GetSection("MinIO"));
builder.Services.AddSingleton<IMinioClient>(provider =>
{
    var config = builder.Configuration.GetSection("MinIO");
    return new MinioClient()
        .WithEndpoint(config.GetValue<string>("Endpoint"))
        .WithCredentials(config.GetValue<string>("AccessKey"), config.GetValue<string>("SecretKey"))
        .WithSSL(config.GetValue<bool>("UseSSL"))
        .Build();
});

// Configure Tesseract
builder.Services.Configure<TesseractConfig>(builder.Configuration.GetSection("Tesseract"));

// Register services
builder.Services.AddScoped<IOcrService, TesseractOcrService>();
builder.Services.AddScoped<IStorageService, MinIOService>();

// Add hosted services
builder.Services.AddHostedService<OcrWorker>();

var host = builder.Build();

try
{
    Log.Information("Starting PaperlessServices");
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PaperlessServices terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
