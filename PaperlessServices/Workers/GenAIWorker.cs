using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaperlessServices.Models;
using PaperlessServices.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaperlessServices.Workers
{
    public class GenAIWorker : BackgroundService
    {
        private readonly ILogger<GenAIWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public GenAIWorker(ILogger<GenAIWorker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
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
                    _logger.LogInformation("Received GenAI message: {Message}", message);

                    var genAIMessage = JsonSerializer.Deserialize<GenAIMessage>(message);
                    if (genAIMessage != null)
                    {
                        await ProcessGenAIMessage(genAIMessage);
                        _channel?.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize GenAI message: {Message}", message);
                        _channel?.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing GenAI message: {Message}", message);
                    _channel?.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel?.BasicConsume(
                queue: "genai.queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("GenAI Worker started and listening for messages");

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task InitializeRabbitMQAsync()
        {
            const int maxRetries = 10;
            var retryDelay = TimeSpan.FromSeconds(5);

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _configuration.GetValue<string>("RabbitMQ:HostName", "localhost"),
                        UserName = _configuration.GetValue<string>("RabbitMQ:UserName", "guest"),
                        Password = _configuration.GetValue<string>("RabbitMQ:Password", "guest"),
                        Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                        AutomaticRecoveryEnabled = true,
                        NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
                    };

                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();

                    // Ensure queues exist
                    _channel.QueueDeclare(
                        queue: "genai.queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _channel.QueueDeclare(
                        queue: "genai_results.queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation("GenAI Worker connected to RabbitMQ successfully");
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

        private async Task ProcessGenAIMessage(GenAIMessage message)
        {
            var result = new GenAIResult
            {
                DocumentId = message.DocumentId,
                CorrelationId = message.CorrelationId,
                ProcessedAt = DateTime.UtcNow
            };

            try
            {
                _logger.LogInformation("Starting AI processing for document {DocumentId}", message.DocumentId);

                // Create a scope for scoped services
                using var scope = _serviceProvider.CreateScope();
                var genAIService = scope.ServiceProvider.GetRequiredService<IGenAIService>();

                // Check if GenAI service is available
                if (!await genAIService.IsAvailableAsync())
                {
                    throw new Exception("GenAI service is not available");
                }

                if (string.IsNullOrWhiteSpace(message.ExtractedText))
                {
                    throw new ArgumentException("No text available for AI processing");
                }

                // Generate summary
                var summary = await genAIService.GenerateSummaryAsync(message.ExtractedText, 300);
                
                // Extract suggested tags
                var suggestedTags = await genAIService.ExtractTagsAsync(message.ExtractedText);

                result.Summary = summary;
                result.SuggestedTags = suggestedTags;
                result.Success = true;

                _logger.LogInformation("AI processing completed for document {DocumentId}. " +
                    "Generated summary of {SummaryLength} characters and {TagCount} tags", 
                    message.DocumentId, summary.Length, suggestedTags.Count);

                // Send result back to result queue
                await SendGenAIResult(result);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
                
                _logger.LogError(ex, "AI processing failed for document {DocumentId}: {ErrorMessage}", 
                    message.DocumentId, ex.Message);

                // Send error result back to result queue
                await SendGenAIResult(result);
            }

            // Log the result
            _logger.LogInformation("GenAI Result for document {DocumentId}: Success={Success}, " +
                "SummaryLength={SummaryLength}, TagCount={TagCount}", 
                result.DocumentId, result.Success, 
                result.Summary?.Length ?? 0, result.SuggestedTags?.Count ?? 0);
        }

        private async Task SendGenAIResult(GenAIResult result)
        {
            try
            {
                var json = JsonSerializer.Serialize(result);
                var body = Encoding.UTF8.GetBytes(json);

                _channel?.BasicPublish(
                    exchange: "",
                    routingKey: "genai_results.queue",
                    basicProperties: null,
                    body: body);

                _logger.LogDebug("Sent GenAI result for document {DocumentId} to result queue", result.DocumentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send GenAI result for document {DocumentId}", result.DocumentId);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GenAI Worker is stopping");
            
            _channel?.Close();
            _connection?.Close();
            
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
