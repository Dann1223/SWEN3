using PaperlessServices.Services.Interfaces;
using Tesseract;

namespace PaperlessServices.Services.Implementations;

public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;
    private readonly string _tessDataPath;

    public TesseractOcrService(ILogger<TesseractOcrService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _tessDataPath = configuration.GetValue<string>("Tesseract:DataPath") ?? "./tessdata";
    }

    public async Task<string> ExtractTextAsync(Stream imageStream, string language = "eng")
    {
        try
        {
            _logger.LogInformation("Starting OCR text extraction with language: {Language}", language);

            // Convert stream to byte array for Tesseract
            var imageBytes = await StreamToByteArrayAsync(imageStream);

            using var engine = new TesseractEngine(_tessDataPath, language, EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            
            var extractedText = page.GetText();
            var confidence = page.GetMeanConfidence();

            _logger.LogInformation("OCR extraction completed. Confidence: {Confidence:F2}, Text length: {TextLength}", 
                confidence, extractedText.Length);

            return extractedText.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from image using OCR");
            throw new InvalidOperationException("OCR text extraction failed", ex);
        }
    }

    public async Task<string> ExtractTextFromPdfAsync(Stream pdfStream, string language = "eng")
    {
        try
        {
            _logger.LogInformation("Starting OCR text extraction from PDF with language: {Language}", language);

            // For now, we'll treat PDF as an image
            // In a production system, you might want to use a PDF library to convert pages to images first
            var extractedText = await ExtractTextAsync(pdfStream, language);

            _logger.LogInformation("PDF OCR extraction completed. Text length: {TextLength}", extractedText.Length);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to extract text from PDF using OCR");
            throw new InvalidOperationException("PDF OCR text extraction failed", ex);
        }
    }

    public async Task<float> GetConfidenceScoreAsync(Stream imageStream)
    {
        try
        {
            var imageBytes = await StreamToByteArrayAsync(imageStream);

            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            using var img = Pix.LoadFromMemory(imageBytes);
            using var page = engine.Process(img);
            
            return page.GetMeanConfidence();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get OCR confidence score");
            return 0.0f;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            // Test OCR availability with a simple test
            using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
            _logger.LogInformation("Tesseract OCR engine is available");
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tesseract OCR engine is not available");
            return await Task.FromResult(false);
        }
    }

    private static async Task<byte[]> StreamToByteArrayAsync(Stream stream)
    {
        if (stream is MemoryStream memoryStream)
        {
            return memoryStream.ToArray();
        }

        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return ms.ToArray();
    }
}
