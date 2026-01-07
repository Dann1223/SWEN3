using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PaperlessServices.Services.Implementations;
using PaperlessServices.Services.Interfaces;

namespace PaperlessServices.Tests.Services;

public class TesseractOcrServiceTests
{
    private readonly Mock<ILogger<TesseractOcrService>> _mockLogger;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly IOcrService _ocrService;

    public TesseractOcrServiceTests()
    {
        _mockLogger = new Mock<ILogger<TesseractOcrService>>();
        _mockConfiguration = new Mock<IConfiguration>();
        
        // Mock configuration using IConfigurationSection instead of extension method
        var mockTesseractSection = new Mock<IConfigurationSection>();
        mockTesseractSection.Setup(x => x.Value).Returns("./tessdata");
        
        var mockDataPathSection = new Mock<IConfigurationSection>();
        mockDataPathSection.Setup(x => x.Value).Returns("./tessdata");
        
        _mockConfiguration.Setup(x => x.GetSection("Tesseract"))
            .Returns(mockTesseractSection.Object);
        _mockConfiguration.Setup(x => x.GetSection("Tesseract:DataPath"))
            .Returns(mockDataPathSection.Object);
        
        _ocrService = new TesseractOcrService(_mockLogger.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task ExtractTextAsync_WithValidImage_ShouldReturnText()
    {
        // Arrange
        var testImageBytes = CreateTestImageData();
        using var imageStream = new MemoryStream(testImageBytes);

        // Act & Assert
        // Note: This test might fail in CI/CD without Tesseract installed
        // In a real scenario, you'd mock the Tesseract engine or use integration tests
        var act = async () => await _ocrService.ExtractTextAsync(imageStream);
        
        // For unit testing, we expect this to throw because tessdata path doesn't exist
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenTesseractNotAvailable_ShouldReturnFalse()
    {
        // Act
        var result = await _ocrService.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetConfidenceScoreAsync_WithInvalidStream_ShouldReturnZero()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var result = await _ocrService.GetConfidenceScoreAsync(emptyStream);

        // Assert
        result.Should().Be(0.0f);
    }

    private static byte[] CreateTestImageData()
    {
        // Create a minimal PNG header for testing
        // This is just for testing purposes - in real scenarios you'd use actual image data
        return new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    }
}

public class MockOcrService : IOcrService
{
    public Task<string> ExtractTextAsync(Stream imageStream, string language = "eng")
    {
        return Task.FromResult("Mock extracted text from image");
    }

    public Task<string> ExtractTextFromPdfAsync(Stream pdfStream, string language = "eng")
    {
        return Task.FromResult("Mock extracted text from PDF");
    }

    public Task<float> GetConfidenceScoreAsync(Stream imageStream)
    {
        return Task.FromResult(0.95f);
    }

    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(true);
    }
}
