using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PaperlessServices.Services.Interfaces;
using PaperlessServices.Workers;
using PaperlessServices.Tests.Services;

namespace PaperlessServices.Tests.Workers;

public class OcrWorkerTests
{
    private readonly Mock<ILogger<OcrWorker>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<IOcrService> _mockOcrService;
    private readonly Mock<IDocumentProcessingService> _mockDocumentProcessingService; 
    private readonly Mock<IStorageService> _mockStorageService;

    public OcrWorkerTests()
    {
        _mockLogger = new Mock<ILogger<OcrWorker>>();
        _mockConfiguration = new Mock<IConfiguration>();
        _mockOcrService = new Mock<IOcrService>();
        _mockDocumentProcessingService = new Mock<IDocumentProcessingService>(); 
        _mockStorageService = new Mock<IStorageService>();

        // Setup configuration using IConfigurationSection instead of extension methods
        var mockRabbitMQSection = new Mock<IConfigurationSection>();

        var mockHostNameSection = new Mock<IConfigurationSection>();
        mockHostNameSection.Setup(x => x.Value).Returns("localhost");

        var mockUserNameSection = new Mock<IConfigurationSection>();
        mockUserNameSection.Setup(x => x.Value).Returns("test");

        var mockPasswordSection = new Mock<IConfigurationSection>();
        mockPasswordSection.Setup(x => x.Value).Returns("test");

        var mockPortSection = new Mock<IConfigurationSection>();
        mockPortSection.Setup(x => x.Value).Returns("5672");

        _mockConfiguration.Setup(x => x.GetSection("RabbitMQ"))
            .Returns(mockRabbitMQSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("RabbitMQ:HostName"))
            .Returns(mockHostNameSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("RabbitMQ:UserName"))
            .Returns(mockUserNameSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("RabbitMQ:Password"))
            .Returns(mockPasswordSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("RabbitMQ:Port"))
            .Returns(mockPortSection.Object);
    }

    [Fact]
    public void OcrWorker_ShouldBeConstructedProperly()
    {
        // Act
        var worker = new OcrWorker(
            _mockLogger.Object,
            _mockConfiguration.Object,
            _mockOcrService.Object,
            _mockDocumentProcessingService.Object, 
            _mockStorageService.Object              
        );

        // Assert
        worker.Should().NotBeNull();
    }

    [Fact]
    public async Task OcrService_ExtractText_ShouldProcessSuccessfully()
    {
        // Arrange
        var mockOcrService = new MockOcrService();
        var testImageData = "test image data"u8.ToArray();
        using var stream = new MemoryStream(testImageData);

        // Act
        var result = await mockOcrService.ExtractTextAsync(stream);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("Mock extracted text from image");
    }

    [Fact]
    public async Task StorageService_FileExists_ShouldReturnCorrectValue()
    {
        // Arrange
        var mockStorageService = new MockStorageService();
        var filePath = "test/document.pdf";
        var testData = "test file content"u8.ToArray();

        mockStorageService.AddMockFile(filePath, testData);

        // Act
        var exists = await mockStorageService.FileExistsAsync(filePath);
        var notExists = await mockStorageService.FileExistsAsync("non-existing-file.pdf");

        // Assert
        exists.Should().BeTrue();
        notExists.Should().BeFalse();
    }

    [Fact]
    public async Task StorageService_DownloadFile_ShouldReturnStream()
    {
        // Arrange
        var mockStorageService = new MockStorageService();
        var filePath = "test/document.pdf";
        var testData = "test file content"u8.ToArray();

        mockStorageService.AddMockFile(filePath, testData);

        // Act
        using var stream = await mockStorageService.DownloadFileAsync(filePath);

        // Assert
        stream.Should().NotBeNull();
        stream.Length.Should().Be(testData.Length);

        // Read and verify content
        using var reader = new BinaryReader(stream);
        var downloadedData = reader.ReadBytes((int)stream.Length);
        downloadedData.Should().BeEquivalentTo(testData);
    }

    [Fact]
    public async Task StorageService_DownloadNonExistingFile_ShouldThrowException()
    {
        // Arrange
        var mockStorageService = new MockStorageService();
        var filePath = "non-existing-file.pdf";

        // Act
        var act = async () => await mockStorageService.DownloadFileAsync(filePath);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage($"File not found: {filePath}");
    }

    [Fact]
    public async Task OcrService_ProcessPdf_ShouldReturnText()
    {
        // Arrange
        var mockOcrService = new MockOcrService();
        var testPdfData = "test pdf data"u8.ToArray();
        using var stream = new MemoryStream(testPdfData);

        // Act
        var result = await mockOcrService.ExtractTextFromPdfAsync(stream);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Be("Mock extracted text from PDF");
    }

    [Fact]
    public async Task OcrService_GetConfidence_ShouldReturnScore()
    {
        // Arrange
        var mockOcrService = new MockOcrService();
        var testImageData = "test image data"u8.ToArray();
        using var stream = new MemoryStream(testImageData);

        // Act
        var confidence = await mockOcrService.GetConfidenceScoreAsync(stream);

        // Assert
        confidence.Should().BeGreaterThan(0);
        confidence.Should().BeLessOrEqualTo(1.0f);
        confidence.Should().Be(0.95f);
    }

    [Fact]
    public async Task OcrService_IsAvailable_ShouldReturnTrue()
    {
        // Arrange
        var mockOcrService = new MockOcrService();

        // Act
        var isAvailable = await mockOcrService.IsAvailableAsync();

        // Assert
        isAvailable.Should().BeTrue();
    }
}

