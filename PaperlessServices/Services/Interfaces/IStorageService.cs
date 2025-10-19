namespace PaperlessServices.Services.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Download a file from storage
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    /// <returns>File stream</returns>
    Task<Stream> DownloadFileAsync(string filePath);

    /// <summary>
    /// Check if file exists in storage
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);
}
