using Microsoft.Extensions.Options;
using NCrontab;
using PaperlessBatchService.Configuration;
using PaperlessBatchService.Services;

namespace PaperlessBatchService.Workers;

/// <summary>
/// Background service that runs batch processing on a schedule
/// </summary>
public class BatchProcessingWorker : BackgroundService
{
    private readonly ILogger<BatchProcessingWorker> _logger;
    private readonly BatchProcessingOptions _options;
    private readonly IServiceProvider _serviceProvider;

    public BatchProcessingWorker(
        IServiceProvider serviceProvider,
        ILogger<BatchProcessingWorker> logger,
        IOptions<BatchProcessingOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.IsEnabled)
        {
            _logger.LogInformation("Batch processing is disabled. Service will exit.");
            return;
        }

        _logger.LogInformation("Batch Processing Worker started with schedule: {CronSchedule}", _options.CronSchedule);

        try
        {
            var crontab = CrontabSchedule.Parse(_options.CronSchedule);
            var nextRun = crontab.GetNextOccurrence(DateTime.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var delay = nextRun - now;

                if (delay > TimeSpan.Zero)
                {
                    _logger.LogInformation("Next batch processing scheduled for: {NextRun}", nextRun);
                    await Task.Delay(delay, stoppingToken);
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                // Execute batch processing
                await RunBatchProcessingAsync();

                // Calculate next run time
                nextRun = crontab.GetNextOccurrence(DateTime.Now);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Batch Processing Worker was cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in Batch Processing Worker");
            throw;
        }
    }

    private async Task RunBatchProcessingAsync()
    {
        try
        {
            _logger.LogInformation("Starting scheduled batch processing at: {StartTime}", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var batchService = scope.ServiceProvider.GetRequiredService<IBatchProcessingService>();
            
            await batchService.ProcessAccessLogFilesAsync();

            _logger.LogInformation("Batch processing completed at: {EndTime}", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during batch processing execution");
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Batch Processing Worker is stopping");
        await base.StopAsync(stoppingToken);
    }
}
