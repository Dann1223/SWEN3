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
    private readonly IDocumentProcessingService _documentProcessingService;
    private readonly IStorageService _storageService;
    private IConnection? _connection;
    private IModel? _channel;

    public OcrWorker(
        ILogger<OcrWorker> logger, 
        IConfiguration configuration,
        IOcrService ocrService,
        IDocumentProcessingService documentProcessingService,
        IStorageService storageService)
    {
        _logger = logger;
        _configuration = configuration;
        _ocrService = ocrService;
        _documentProcessingService = documentProcessingService;
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

            // Check if the document type is supported
            if (!_documentProcessingService.CanProcess(message.FileName))
            {
                throw new InvalidOperationException($"Unsupported file type: {message.FileType}");
            }

            var processingMethod = _documentProcessingService.GetProcessingMethod(message.FileName);
            _logger.LogInformation("Using processing method: {ProcessingMethod} for file: {FileName}", 
                processingMethod, message.FileName);

            // Check if OCR service is available (still needed for image processing)
            if (!await _ocrService.IsAvailableAsync())
            {
                _logger.LogWarning("Tesseract OCR service is not available, but continuing with document processing");
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

            // Use the smart document processing service
            string extractedText = await _documentProcessingService.ExtractTextAsync(
                fileStream, message.FileName, "eng");

            // Try to get confidence score for image-based processing
            float confidence = 0.0f;
            try
            {
                if (message.FileType.ToLowerInvariant() != ".pdf")
                {
                    fileStream.Position = 0; // Reset stream position
                    confidence = await _ocrService.GetConfidenceScoreAsync(fileStream);
                }
                else
                {
                    // For PDF, set a default confidence based on text extraction success
                    confidence = !string.IsNullOrWhiteSpace(extractedText) && 
                               !extractedText.StartsWith("[") ? 0.95f : 0.0f;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not determine confidence score for document {DocumentId}", message.DocumentId);
                confidence = 0.0f;
            }

            result.ExtractedText = extractedText;
            result.Confidence = confidence;
            result.Success = true;

            _logger.LogInformation("OCR processing completed for document {DocumentId}. " +
                "Extracted {TextLength} characters with confidence {Confidence:F2}", 
                message.DocumentId, extractedText.Length, confidence);

            // Send result back to result queue for database update
            await SendOcrResult(result, processingMethod);
            
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            
            _logger.LogError(ex, "OCR processing failed for document {DocumentId}: {ErrorMessage}", 
                message.DocumentId, ex.Message);

            // Send error result back to result queue
            await SendOcrResult(result, _documentProcessingService.GetProcessingMethod(message.FileName));
        }

        // For now, just log the result
        _logger.LogInformation("OCR Result for document {DocumentId}: Success={Success}, " +
            "TextLength={TextLength}, Confidence={Confidence:F2}", 
            result.DocumentId, result.Success, result.ExtractedText.Length, result.Confidence);
    }

    private async Task SendOcrResult(OcrResult ocrResult, string processingMethod)
    {
        try
        {
            var resultMessage = new OcrResultMessage
            {
                DocumentId = ocrResult.DocumentId,
                CorrelationId = ocrResult.CorrelationId,
                Success = ocrResult.Success,
                ExtractedText = ocrResult.ExtractedText,
                Confidence = ocrResult.Confidence,
                ErrorMessage = ocrResult.ErrorMessage,
                ProcessedAt = ocrResult.ProcessedAt,
                ProcessingMethod = processingMethod
            };

            // Declare the OCR result queue
            _channel!.QueueDeclare(
                queue: "ocr_results",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var jsonMessage = JsonSerializer.Serialize(resultMessage);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "ocr_results",
                basicProperties: null,
                body: body);

            _logger.LogInformation("Sent OCR result for document {DocumentId} to result queue", ocrResult.DocumentId);

            // If OCR was successful and we have extracted text, send to GenAI queue for summarization
            if (ocrResult.Success && !string.IsNullOrWhiteSpace(ocrResult.ExtractedText))
            {
                await SendToGenAIQueue(ocrResult);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OCR result for document {DocumentId}", ocrResult.DocumentId);
        }
    }

    private async Task SendToGenAIQueue(OcrResult ocrResult)
    {
        try
        {
            var genAIMessage = new GenAIMessage
            {
                DocumentId = ocrResult.DocumentId,
                CorrelationId = ocrResult.CorrelationId,
                ExtractedText = ocrResult.ExtractedText,
                CreatedAt = DateTime.UtcNow
            };

            // Declare the GenAI queue
            _channel!.QueueDeclare(
                queue: "genai.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var jsonMessage = JsonSerializer.Serialize(genAIMessage);
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "genai.queue",
                basicProperties: null,
                body: body);

            _logger.LogInformation("Sent document {DocumentId} to GenAI queue for AI processing", ocrResult.DocumentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send document {DocumentId} to GenAI queue", ocrResult.DocumentId);
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
