using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using PaperlessServices.Services.Interfaces;
using PaperlessServices.Models;

namespace PaperlessServices.Workers;

public class OcrWorker : BackgroundService
{
    private readonly ILogger<OcrWorker> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOcrService _ocrService;
    private readonly IStorageService _storageService;
    private IConnection? _connection;
    private IModel? _channel;

    public OcrWorker(
        ILogger<OcrWorker> logger, 
        IConfiguration configuration,
        IOcrService ocrService,
        IStorageService storageService)
    {
        _logger = logger;
        _configuration = configuration;
        _ocrService = ocrService;
        _storageService = storageService;
    }

    private async Task InitializeRabbitMQAsync()
    {
        var maxRetries = 30; // 最多重试30次 (1分钟)
        var retryDelay = TimeSpan.FromSeconds(2); // 每次重试间隔2秒
        
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation("Attempting to connect to RabbitMQ (attempt {Attempt}/{MaxRetries})", attempt, maxRetries);
                
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

                // Ensure queue exists
                _channel.QueueDeclare(
                    queue: "ocr.queue",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation("OCR Worker connected to RabbitMQ successfully");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to connect to RabbitMQ (attempt {Attempt}/{MaxRetries}). Retrying in {Delay} seconds...", 
                    attempt, maxRetries, retryDelay.TotalSeconds);
                
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex, "Failed to connect to RabbitMQ after {MaxRetries} attempts", maxRetries);
                    throw;
                }
                
                await Task.Delay(retryDelay);
            }
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 等待 RabbitMQ 连接建立
        await InitializeRabbitMQAsync();
        
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (sender, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            try
            {
                _logger.LogInformation("Received OCR message: {Message}", message);

                var ocrMessage = JsonSerializer.Deserialize<OcrMessage>(message, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (ocrMessage != null)
                {
                    await ProcessOcrMessage(ocrMessage);
                    _channel?.BasicAck(ea.DeliveryTag, false);
                    _logger.LogInformation("Successfully processed OCR for document {DocumentId}", ocrMessage.DocumentId);
                }
                else
                {
                    _logger.LogWarning("Failed to deserialize OCR message");
                    _channel?.BasicNack(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing OCR message: {Message}", message);
                _channel?.BasicNack(ea.DeliveryTag, false, false);
            }
        };

        _channel?.BasicConsume(
            queue: "ocr.queue",
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("OCR Worker started and listening for messages");

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessOcrMessage(OcrMessage message)
    {
        var result = new OcrResult
        {
            DocumentId = message.DocumentId,
            CorrelationId = message.CorrelationId,
            ProcessedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting OCR processing for document {DocumentId}, file: {FileName}", 
                message.DocumentId, message.FileName);

            // Check if OCR service is available
            if (!await _ocrService.IsAvailableAsync())
            {
                throw new InvalidOperationException("OCR service is not available");
            }

            // Check if file exists in storage
            if (!await _storageService.FileExistsAsync(message.FilePath))
            {
                throw new FileNotFoundException($"File not found in storage: {message.FilePath}");
            }

            // Download file from MinIO
            using var fileStream = await _storageService.DownloadFileAsync(message.FilePath);
            
            _logger.LogInformation("Downloaded file {FileName} from storage, size: {Size} bytes", 
                message.FileName, fileStream.Length);

            // Perform OCR based on file type
            string extractedText;
            if (message.FileType.ToLowerInvariant() == ".pdf")
            {
                extractedText = await _ocrService.ExtractTextFromPdfAsync(fileStream);
            }
            else
            {
                extractedText = await _ocrService.ExtractTextAsync(fileStream);
            }

            // Get confidence score
            fileStream.Position = 0; // Reset stream position
            var confidence = await _ocrService.GetConfidenceScoreAsync(fileStream);

            result.ExtractedText = extractedText;
            result.Confidence = confidence;
            result.Success = true;

            _logger.LogInformation("OCR processing completed for document {DocumentId}. " +
                "Extracted {TextLength} characters with confidence {Confidence:F2}", 
                message.DocumentId, extractedText.Length, confidence);

            // TODO: Send result back to result queue for database update
            // This will be implemented when we add result processing
            
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "OCR processing failed for document {DocumentId}: {ErrorMessage}", 
                message.DocumentId, ex.Message);
        }

        // For now, just log the result
        _logger.LogInformation("OCR Result for document {DocumentId}: Success={Success}, " +
            "TextLength={TextLength}, Confidence={Confidence:F2}", 
            result.DocumentId, result.Success, result.ExtractedText.Length, result.Confidence);
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
