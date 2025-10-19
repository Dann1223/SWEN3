using Minio;
using Minio.DataModel.Args;
using PaperlessServices.Services.Interfaces;

namespace PaperlessServices.Services.Implementations;

public class MinIOService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly ILogger<MinIOService> _logger;
    private readonly string _bucketName;

    public MinIOService(IMinioClient minioClient, ILogger<MinIOService> logger, IConfiguration configuration)
    {
        _minioClient = minioClient;
        _logger = logger;
        _bucketName = configuration.GetValue<string>("MinIO:BucketName") ?? "documents";
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Downloading file from MinIO at path {FilePath}", filePath);

            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filePath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully downloaded file from MinIO at path {FilePath}, Size: {Size} bytes", 
                filePath, memoryStream.Length);
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from MinIO at path {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to download file from storage: {filePath}", ex);
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(filePath);

            await _minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
