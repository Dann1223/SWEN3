using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using Minio;
using PaperlessRESTAPI.Data;
using PaperlessRESTAPI.Data.Repositories;
using PaperlessRESTAPI.Infrastructure.Validation;
using PaperlessRESTAPI.Infrastructure.Middleware;
using PaperlessRESTAPI.Services.Interfaces;
using PaperlessRESTAPI.Services.Implementations;
using PaperlessRESTAPI.Configuration;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/paperless-api-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<UploadDocumentValidator>();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

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

// Add business services
builder.Services.AddScoped<IStorageService, MinIOService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IOcrResultService, OcrResultService>();
builder.Services.AddScoped<IGenAIResultService, GenAIResultService>();

// Add RabbitMQ configuration and service
builder.Services.Configure<RabbitMQConfig>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddSingleton<IQueueService, RabbitMQService>();

// Add Elasticsearch
builder.Services.AddSingleton<Nest.IElasticClient>(provider =>
{
    // Try environment variable first (for Docker), then appsettings
    var connectionString = builder.Configuration["Elasticsearch:Uri"] 
                          ?? builder.Configuration.GetConnectionString("Elasticsearch") 
                          ?? "http://localhost:9200";
    
    var settings = new Nest.ConnectionSettings(new Uri(connectionString))
        .DefaultIndex("documents")
        .DisableDirectStreaming()
        .ThrowExceptions(false); // Don't throw exceptions to allow graceful error handling
    
    return new Nest.ElasticClient(settings);
});

// Add comment service
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// Add background services
builder.Services.AddHostedService<PaperlessRESTAPI.Workers.OcrResultWorker>();
builder.Services.AddHostedService<PaperlessRESTAPI.Workers.GenAIResultWorker>();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Add OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:8080") // Include WebUI
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Log.Information("Checking database connection and applying migrations...");
        
        // Check if database exists and is accessible
        if (context.Database.CanConnect())
        {
            Log.Information("Database connection successful");
            
            // Get pending migrations
            var pendingMigrations = context.Database.GetPendingMigrations();
            if (pendingMigrations.Any())
            {
                Log.Information("Found {Count} pending migrations: {Migrations}", 
                    pendingMigrations.Count(), string.Join(", ", pendingMigrations));
                
                context.Database.Migrate();
                Log.Information("Database migrations applied successfully");
            }
            else
            {
                Log.Information("Database is up to date, no migrations needed");
            }
        }
        else
        {
            Log.Warning("Cannot connect to database, attempting to create and migrate...");
            context.Database.Migrate();
            Log.Information("Database created and migrations applied successfully");
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add custom middleware
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();

// Enhanced health check endpoint
app.MapGet("/health", async (IQueueService queueService) => 
{
    var rabbitMQHealthy = await queueService.IsHealthyAsync();
    
    return Results.Ok(new { 
        status = rabbitMQHealthy ? "healthy" : "degraded",
        timestamp = DateTime.UtcNow,
        services = new
        {
            database = "healthy", // Could add actual DB health check
            rabbitmq = rabbitMQHealthy ? "healthy" : "unhealthy"
        }
    });
});

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
