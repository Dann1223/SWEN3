namespace PaperlessRESTAPI.Services.Interfaces;

public interface IStorageService
{
    /// <summary>
    /// Upload a file to storage
    /// </summary>
    /// <param name="stream">File stream</param>
    /// <param name="fileName">File name</param>
    /// <param name="contentType">Content type</param>
    /// <returns>File path in storage</returns>
    Task<string> UploadFileAsync(Stream stream, string fileName, string contentType);

    /// <summary>
    /// Download a file from storage
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    /// <returns>File stream</returns>
    Task<Stream> DownloadFileAsync(string filePath);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Check if file exists in storage
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string filePath);

    /// <summary>
    /// Get file URL
    /// </summary>
    /// <param name="filePath">File path in storage</param>
    /// <returns>File URL</returns>
    Task<string> GetFileUrlAsync(string filePath);
}
