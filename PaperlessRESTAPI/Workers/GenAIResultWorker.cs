using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PaperlessRESTAPI.Models;
using PaperlessRESTAPI.Services.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PaperlessRESTAPI.Workers
{
    public class GenAIResultWorker : BackgroundService
    {
        private readonly ILogger<GenAIResultWorker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public GenAIResultWorker(
            ILogger<GenAIResultWorker> logger,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
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
                    _logger.LogInformation("Received GenAI result message: {Message}", message);

                    var genAIResult = JsonSerializer.Deserialize<GenAIResultMessage>(message);
                    if (genAIResult != null)
                    {
                        await ProcessGenAIResult(genAIResult);
                        _channel?.BasicAck(ea.DeliveryTag, false);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize GenAI result message: {Message}", message);
                        _channel?.BasicNack(ea.DeliveryTag, false, false);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing GenAI result message: {Message}", message);
                    _channel?.BasicNack(ea.DeliveryTag, false, false);
                }
            };

            _channel?.BasicConsume(
                queue: "genai_results.queue",
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("GenAI Result Worker started and listening for messages");

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

                    // Ensure queue exists
                    _channel.QueueDeclare(
                        queue: "genai_results.queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _logger.LogInformation("GenAI Result Worker connected to RabbitMQ successfully");
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

        private async Task ProcessGenAIResult(GenAIResultMessage result)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var genAIResultService = scope.ServiceProvider.GetRequiredService<IGenAIResultService>();

                var success = await genAIResultService.ProcessGenAIResultAsync(result);
                
                if (success)
                {
                    _logger.LogInformation("Successfully processed GenAI result for document {DocumentId}", result.DocumentId);
                }
                else
                {
                    _logger.LogWarning("Failed to process GenAI result for document {DocumentId}", result.DocumentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GenAI result for document {DocumentId}", result.DocumentId);
                throw; // Re-throw to trigger message retry/dead letter handling
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("GenAI Result Worker is stopping");
            
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
