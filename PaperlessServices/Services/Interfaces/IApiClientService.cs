namespace PaperlessServices.Services.Interfaces;

public interface IApiClientService
{
    /// <summary>
    /// Send OCR result to the API for database update
    /// </summary>
    /// <param name="ocrResult">The OCR result to send</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SendOcrResultAsync(Models.OcrResult ocrResult);
}
