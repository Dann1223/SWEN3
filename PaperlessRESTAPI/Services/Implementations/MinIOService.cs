using Minio;
using Minio.DataModel.Args;
using PaperlessRESTAPI.Configuration;
using PaperlessRESTAPI.Services.Interfaces;
using PaperlessRESTAPI.Infrastructure.Exceptions;
using Microsoft.Extensions.Options;

namespace PaperlessRESTAPI.Services.Implementations;

public class MinIOService : IStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly MinIOConfig _config;
    private readonly ILogger<MinIOService> _logger;

    public MinIOService(IMinioClient minioClient, IOptions<MinIOConfig> config, ILogger<MinIOService> logger)
    {
        _minioClient = minioClient;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType)
    {
        try
        {
            // Ensure bucket exists
            await EnsureBucketExistsAsync();

            // Generate unique file path
            var fileExtension = Path.GetExtension(fileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = $"documents/{DateTime.UtcNow:yyyy/MM/dd}/{uniqueFileName}";

            _logger.LogInformation("Uploading file {FileName} to MinIO at path {FilePath}", fileName, filePath);

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath)
                .WithStreamData(stream)
                .WithObjectSize(stream.Length)
                .WithContentType(contentType)
                .WithHeaders(new Dictionary<string, string>
                {
                    ["x-amz-meta-original-filename"] = fileName,
                    ["x-amz-meta-upload-date"] = DateTime.UtcNow.ToString("O")
                });

            await _minioClient.PutObjectAsync(putObjectArgs);

            _logger.LogInformation("Successfully uploaded file {FileName} to MinIO at path {FilePath}", fileName, filePath);
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to MinIO", fileName);
            throw new ServiceException("Failed to upload file to storage", ex);
        }
    }

    public async Task<Stream> DownloadFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Downloading file from MinIO at path {FilePath}", filePath);

            var memoryStream = new MemoryStream();
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath)
                .WithCallbackStream(stream => stream.CopyTo(memoryStream));

            await _minioClient.GetObjectAsync(getObjectArgs);
            memoryStream.Position = 0;

            _logger.LogInformation("Successfully downloaded file from MinIO at path {FilePath}", filePath);
            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download file from MinIO at path {FilePath}", filePath);
            throw new ServiceException($"Failed to download file from storage: {filePath}", ex);
        }
    }

    public async Task DeleteFileAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Deleting file from MinIO at path {FilePath}", filePath);

            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath);

            await _minioClient.RemoveObjectAsync(removeObjectArgs);

            _logger.LogInformation("Successfully deleted file from MinIO at path {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file from MinIO at path {FilePath}", filePath);
            throw new ServiceException($"Failed to delete file from storage: {filePath}", ex);
        }
    }

    public async Task<bool> FileExistsAsync(string filePath)
    {
        try
        {
            var statObjectArgs = new StatObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath);

            await _minioClient.StatObjectAsync(statObjectArgs);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<string> GetFileUrlAsync(string filePath)
    {
        try
        {
            var presignedGetObjectArgs = new PresignedGetObjectArgs()
                .WithBucket(_config.BucketName)
                .WithObject(filePath)
                .WithExpiry(60 * 60 * 24); // 24 hours

            var url = await _minioClient.PresignedGetObjectAsync(presignedGetObjectArgs);
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate URL for file at path {FilePath}", filePath);
            throw new ServiceException($"Failed to generate file URL: {filePath}", ex);
        }
    }

    private async Task EnsureBucketExistsAsync()
    {
        try
        {
            var bucketExistsArgs = new BucketExistsArgs().WithBucket(_config.BucketName);
            var bucketExists = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!bucketExists)
            {
                _logger.LogInformation("Creating bucket {BucketName} in MinIO", _config.BucketName);
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_config.BucketName);
                await _minioClient.MakeBucketAsync(makeBucketArgs);
                _logger.LogInformation("Successfully created bucket {BucketName} in MinIO", _config.BucketName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to ensure bucket {BucketName} exists in MinIO", _config.BucketName);
            throw new ServiceException($"Failed to ensure bucket exists: {_config.BucketName}", ex);
        }
    }
}
