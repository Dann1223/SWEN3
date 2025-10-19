using Microsoft.Extensions.Options;
using PaperlessRESTAPI.Configuration;
using PaperlessRESTAPI.Infrastructure.Exceptions;
using PaperlessRESTAPI.Models.Messages;
using PaperlessRESTAPI.Services.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PaperlessRESTAPI.Services.Implementations;

public class RabbitMQService : IQueueService, IDisposable
{
    private readonly RabbitMQConfig _config;
    private readonly ILogger<RabbitMQService> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private readonly object _lock = new();

    public RabbitMQService(IOptions<RabbitMQConfig> config, ILogger<RabbitMQService> logger)
    {
        _config = config.Value;
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _config.HostName,
                UserName = _config.UserName,
                Password = _config.Password,
                Port = _config.Port,
                VirtualHost = _config.VirtualHost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare exchanges
            _channel.ExchangeDeclare(_config.DocumentExchange, ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(_config.ProcessingExchange, ExchangeType.Topic, durable: true);

            // Declare queues
            DeclareQueue(_config.OcrQueue);
            DeclareQueue(_config.GenAIQueue);
            DeclareQueue(_config.IndexingQueue);
            DeclareQueue(_config.ResultQueue);

            // Bind queues to exchanges
            _channel.QueueBind(_config.OcrQueue, _config.DocumentExchange, "document.ocr");
            _channel.QueueBind(_config.GenAIQueue, _config.ProcessingExchange, "processing.genai");
            _channel.QueueBind(_config.IndexingQueue, _config.ProcessingExchange, "processing.indexing");
            _channel.QueueBind(_config.ResultQueue, _config.ProcessingExchange, "processing.result");

            _logger.LogInformation("RabbitMQ connection established successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw new ServiceException("Failed to connect to RabbitMQ", ex);
        }
    }

    private void DeclareQueue(string queueName)
    {
        _channel?.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen != true || _channel?.IsOpen != true)
        {
            lock (_lock)
            {
                if (_connection?.IsOpen != true || _channel?.IsOpen != true)
                {
                    _logger.LogWarning("RabbitMQ connection lost, attempting to reconnect...");
                    Dispose();
                    InitializeConnection();
                }
            }
        }
    }

    public async Task SendOcrMessageAsync(OcrMessage message)
    {
        await SendMessageAsync(_config.DocumentExchange, "document.ocr", message);
        _logger.LogInformation("OCR message sent for document {DocumentId}, CorrelationId: {CorrelationId}", 
            message.DocumentId, message.CorrelationId);
    }

    public async Task SendGenAIMessageAsync(GenAIMessage message)
    {
        await SendMessageAsync(_config.ProcessingExchange, "processing.genai", message);
        _logger.LogInformation("GenAI message sent for document {DocumentId}, CorrelationId: {CorrelationId}", 
            message.DocumentId, message.CorrelationId);
    }

    public async Task SendIndexingMessageAsync(IndexingMessage message)
    {
        await SendMessageAsync(_config.ProcessingExchange, "processing.indexing", message);
        _logger.LogInformation("Indexing message sent for document {DocumentId}, CorrelationId: {CorrelationId}", 
            message.DocumentId, message.CorrelationId);
    }

    private async Task SendMessageAsync<T>(string exchange, string routingKey, T message)
    {
        try
        {
            EnsureConnection();

            var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            var body = Encoding.UTF8.GetBytes(json);

            var properties = _channel!.CreateBasicProperties();
            properties.Persistent = true;
            properties.MessageId = Guid.NewGuid().ToString();
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            properties.ContentType = "application/json";

            if (message is OcrMessage ocrMsg)
                properties.CorrelationId = ocrMsg.CorrelationId;
            else if (message is GenAIMessage genAIMsg)
                properties.CorrelationId = genAIMsg.CorrelationId;
            else if (message is IndexingMessage indexMsg)
                properties.CorrelationId = indexMsg.CorrelationId;

            _channel.BasicPublish(
                exchange: exchange,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send message to exchange {Exchange} with routing key {RoutingKey}", 
                exchange, routingKey);
            throw new ServiceException($"Failed to send message to queue", ex);
        }
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            return await Task.FromResult(_connection?.IsOpen == true && _channel?.IsOpen == true);
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
            _channel?.Dispose();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ connection");
        }
    }
}
