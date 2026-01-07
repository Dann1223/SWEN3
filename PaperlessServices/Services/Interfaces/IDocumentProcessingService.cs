namespace PaperlessServices.Services.Interfaces;

public interface IDocumentProcessingService
{
    /// <summary>
    /// Extract text from a document based on its file type
    /// </summary>
    /// <param name="documentStream">The document stream</param>
    /// <param name="fileName">The file name to determine type</param>
    /// <param name="language">OCR language (used for image-based extraction)</param>
    /// <returns>Extracted text content</returns>
    Task<string> ExtractTextAsync(Stream documentStream, string fileName, string language = "eng");

    /// <summary>
    /// Check if the service can process the given file type
    /// </summary>
    /// <param name="fileName">The file name to check</param>
    /// <returns>True if the file type is supported</returns>
    bool CanProcess(string fileName);

    /// <summary>
    /// Get the processing method used for a file type
    /// </summary>
    /// <param name="fileName">The file name to check</param>
    /// <returns>The processing method description</returns>
    string GetProcessingMethod(string fileName);
}
