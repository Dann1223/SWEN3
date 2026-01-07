using FluentAssertions;
using PaperlessServices.Models;

namespace PaperlessServices.Tests.Models;

public class MessagesTests
{
    [Fact]
    public void OcrMessage_ShouldHaveCorrectProperties()
    {
        // Arrange & Act
        var message = new OcrMessage
        {
            DocumentId = 123,
            FileName = "test-document.pdf",
            FilePath = "documents/2024/01/01/test-document.pdf",
            FileType = ".pdf",
            RequestedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.DocumentId.Should().Be(123);
        message.FileName.Should().Be("test-document.pdf");
        message.FilePath.Should().Be("documents/2024/01/01/test-document.pdf");
        message.FileType.Should().Be(".pdf");
        message.RequestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        message.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OcrResult_ShouldHaveCorrectDefaultValues()
    {
        // Arrange & Act
        var result = new OcrResult();

        // Assert
        result.DocumentId.Should().Be(0);
        result.ExtractedText.Should().BeEmpty();
        result.Confidence.Should().Be(0.0f);
        result.CorrelationId.Should().BeEmpty();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void OcrResult_ShouldAllowSuccessfulResult()
    {
        // Arrange & Act
        var result = new OcrResult
        {
            DocumentId = 456,
            ExtractedText = "This is extracted text from OCR",
            Confidence = 0.95f,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = "test-correlation-id",
            Success = true,
            ErrorMessage = null
        };

        // Assert
        result.DocumentId.Should().Be(456);
        result.ExtractedText.Should().Be("This is extracted text from OCR");
        result.Confidence.Should().Be(0.95f);
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CorrelationId.Should().Be("test-correlation-id");
        result.Success.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void OcrResult_ShouldAllowFailedResult()
    {
        // Arrange & Act
        var result = new OcrResult
        {
            DocumentId = 789,
            ExtractedText = "",
            Confidence = 0.0f,
            ProcessedAt = DateTime.UtcNow,
            CorrelationId = "failed-correlation-id",
            Success = false,
            ErrorMessage = "OCR processing failed due to invalid image format"
        };

        // Assert
        result.DocumentId.Should().Be(789);
        result.ExtractedText.Should().BeEmpty();
        result.Confidence.Should().Be(0.0f);
        result.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.CorrelationId.Should().Be("failed-correlation-id");
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Be("OCR processing failed due to invalid image format");
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".jpg")]
    [InlineData(".png")]
    [InlineData(".tiff")]
    public void OcrMessage_ShouldSupportDifferentFileTypes(string fileType)
    {
        // Arrange & Act
        var message = new OcrMessage
        {
            DocumentId = 1,
            FileName = $"test{fileType}",
            FileType = fileType,
            RequestedAt = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid().ToString()
        };

        // Assert
        message.FileType.Should().Be(fileType);
        message.FileName.Should().EndWith(fileType);
        
        // Verify common file types are supported
        var supportedTypes = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp", ".gif" };
        supportedTypes.Should().Contain(fileType);
    }

    [Fact]
    public void OcrMessage_CorrelationId_ShouldBeUnique()
    {
        // Arrange & Act
        var message1 = new OcrMessage { CorrelationId = Guid.NewGuid().ToString() };
        var message2 = new OcrMessage { CorrelationId = Guid.NewGuid().ToString() };

        // Assert
        message1.CorrelationId.Should().NotBe(message2.CorrelationId);
        message1.CorrelationId.Should().NotBeNullOrEmpty();
        message2.CorrelationId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void OcrResult_ConfidenceRange_ShouldBeValid()
    {
        // Arrange & Act
        var lowConfidenceResult = new OcrResult { Confidence = 0.1f };
        var mediumConfidenceResult = new OcrResult { Confidence = 0.5f };
        var highConfidenceResult = new OcrResult { Confidence = 0.95f };
        var perfectConfidenceResult = new OcrResult { Confidence = 1.0f };

        // Assert
        lowConfidenceResult.Confidence.Should().BeInRange(0.0f, 1.0f);
        mediumConfidenceResult.Confidence.Should().BeInRange(0.0f, 1.0f);
        highConfidenceResult.Confidence.Should().BeInRange(0.0f, 1.0f);
        perfectConfidenceResult.Confidence.Should().BeInRange(0.0f, 1.0f);
    }
}
