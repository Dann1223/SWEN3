using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using PaperlessRESTAPI.Models.DTOs;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Workers;

/// <summary>
/// Background service to process OCR results from RabbitMQ
/// </summary>
public class OcrResultWorker : BackgroundService
{
    private readonly ILogger<OcrResultWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IModel? _channel;

    public OcrResultWorker(
        ILogger<OcrResultWorker> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    private async Task InitializeRabbitMQAsync()
    {
        var maxRetries = 30;
        var retryDelay = TimeSpan.FromSeconds(2);
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ for OCR results (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                
                var factory = new ConnectionFactory
                {
                    HostName = _configuration.GetValue<string>("RabbitMQ:HostName") ?? "rabbitmq",
                    UserName = _configuration.GetValue<string>("RabbitMQ:UserName") ?? "guest",
                    Password = _configuration.GetValue<string>("RabbitMQ:Password") ?? "guest",
                    Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declare the OCR result queue
                _channel.QueueDeclare(
                    queue: "ocr_results",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("OCR Result Worker connected to RabbitMQ successfully");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                
                if (attempt == maxRetries)
                {
                    _logger.LogError("Failed to connect to RabbitMQ after {MaxRetries} attempts", maxRetries);
                    throw;
                }
                
                await Task.Delay(retryDelay);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitializeRabbitMQAsync();

        var consumer = new EventingBasicConsumer(_channel!);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                _logger.LogInformation("Received OCR result message: {Message}", message);

                var ocrResult = JsonSerializer.Deserialize<OcrResultMessage>(message);
                if (ocrResult != null)
                {
                    await ProcessOcrResult(ocrResult);
                }

                _channel!.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR result message");
                _channel!.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            }
        };

        _channel!.BasicConsume(queue: "ocr_results", autoAck: false, consumer: consumer);
        
        _logger.LogInformation("OCR Result Worker started and listening for messages");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessOcrResult(OcrResultMessage ocrResult)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var ocrResultService = scope.ServiceProvider.GetRequiredService<IOcrResultService>();

            var success = await ocrResultService.ProcessOcrResultAsync(ocrResult);
            
            if (success)
            {
                _logger.LogInformation("Successfully processed OCR result for document {DocumentId}", ocrResult.DocumentId);
            }
            else
            {
                _logger.LogWarning("Failed to process OCR result for document {DocumentId}", ocrResult.DocumentId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OCR result for document {DocumentId}", ocrResult.DocumentId);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
