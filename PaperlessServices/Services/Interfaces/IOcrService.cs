namespace PaperlessServices.Services.Interfaces;

public interface IOcrService
{
    /// <summary>
    /// Extract text from image stream using OCR
    /// </summary>
    /// <param name="imageStream">Image stream to process</param>
    /// <param name="language">OCR language (default: eng)</param>
    /// <returns>Extracted text</returns>
    Task<string> ExtractTextAsync(Stream imageStream, string language = "eng");

    /// <summary>
    /// Extract text from PDF stream using OCR
    /// </summary>
    /// <param name="pdfStream">PDF stream to process</param>
    /// <param name="language">OCR language (default: eng)</param>
    /// <returns>Extracted text</returns>
    Task<string> ExtractTextFromPdfAsync(Stream pdfStream, string language = "eng");

    /// <summary>
    /// Get confidence score of OCR result
    /// </summary>
    /// <param name="imageStream">Image stream to analyze</param>
    /// <returns>Confidence score (0-100)</returns>
    Task<float> GetConfidenceScoreAsync(Stream imageStream);

    /// <summary>
    /// Check if OCR engine is available and working
    /// </summary>
    /// <returns>True if OCR is available</returns>
    Task<bool> IsAvailableAsync();
}
