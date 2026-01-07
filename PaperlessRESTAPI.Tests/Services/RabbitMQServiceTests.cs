using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PaperlessRESTAPI.Configuration;
using PaperlessRESTAPI.Models.Messages;
using PaperlessRESTAPI.Services.Implementations;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Tests.Services;

/// <summary>
/// Unit tests for RabbitMQ related components (without real connections)
/// </summary>
public class RabbitMQServiceTests
{
    [Fact]
    public void RabbitMQConfig_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var config = new RabbitMQConfig
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest",
            Port = 5672,
            VirtualHost = "/",
            OcrQueue = "ocr-queue",
            GenAIQueue = "genai-queue",
            IndexingQueue = "indexing-queue"
        };

        // Assert
        config.HostName.Should().Be("localhost");
        config.UserName.Should().Be("guest");
        config.Password.Should().Be("guest");
        config.Port.Should().Be(5672);
        config.VirtualHost.Should().Be("/");
        config.OcrQueue.Should().Be("ocr-queue");
        config.GenAIQueue.Should().Be("genai-queue");
        config.IndexingQueue.Should().Be("indexing-queue");
    }

    [Fact]
    public void RabbitMQService_ShouldImplementIQueueService()
    {
        // Assert
        typeof(RabbitMQService).Should().BeAssignableTo<IQueueService>();
    }

    [Fact]
    public void RabbitMQService_ShouldImplementIDisposable()
    {
        // Assert
        typeof(RabbitMQService).Should().BeAssignableTo<IDisposable>();
    }

    [Fact]
    public void RabbitMQService_ShouldHaveCorrectConstructorParameters()
    {
        // Arrange
        var constructors = typeof(RabbitMQService).GetConstructors();

        // Assert
        constructors.Should().HaveCount(1);
        var constructor = constructors.First();
        var parameters = constructor.GetParameters();
        
        parameters.Should().HaveCount(2);
        parameters[0].ParameterType.Should().Be(typeof(IOptions<RabbitMQConfig>));
        parameters[1].ParameterType.Should().Be(typeof(ILogger<RabbitMQService>));
    }

    [Fact]
    public void IQueueService_ShouldHaveCorrectMethods()
    {
        // Arrange
        var interfaceType = typeof(IQueueService);

        // Assert
        interfaceType.GetMethod("SendOcrMessageAsync").Should().NotBeNull();
        interfaceType.GetMethod("SendGenAIMessageAsync").Should().NotBeNull();
        interfaceType.GetMethod("SendIndexingMessageAsync").Should().NotBeNull();
        interfaceType.GetMethod("IsHealthyAsync").Should().NotBeNull();
    }

    [Fact]
    public void OcrMessage_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new OcrMessage
        {
            DocumentId = 1,
            FilePath = "test.pdf",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.DocumentId.Should().Be(1);
        message.FilePath.Should().Be("test.pdf");
        message.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenAIMessage_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new GenAIMessage
        {
            DocumentId = 1,
            OcrText = "Test content",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.DocumentId.Should().Be(1);
        message.OcrText.Should().Be("Test content");
        message.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void IndexingMessage_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new IndexingMessage
        {
            DocumentId = 1,
            OcrText = "Test content",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.DocumentId.Should().Be(1);
        message.OcrText.Should().Be("Test content");
        message.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ProcessingResultMessage_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new ProcessingResultMessage
        {
            DocumentId = 1,
            ProcessingType = "OCR",
            IsSuccess = true,
            Result = "Text extracted successfully",
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.DocumentId.Should().Be(1);
        message.ProcessingType.Should().Be("OCR");
        message.IsSuccess.Should().BeTrue();
        message.Result.Should().Be("Text extracted successfully");
        message.CorrelationId.Should().NotBeNullOrEmpty();
    }
}
