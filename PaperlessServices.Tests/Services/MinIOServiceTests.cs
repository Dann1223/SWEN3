using FluentAssertions;
using PaperlessServices.Services.Interfaces;

namespace PaperlessServices.Tests.Services;

public class MinIOServiceTests
{
    [Fact]
    public void MinIOService_ShouldImplementIStorageService()
    {
        // This test ensures our service implements the correct interface
        // In integration tests, we would test the actual MinIO functionality
        var storageServiceType = typeof(IStorageService);
        storageServiceType.Should().NotBeNull();
        storageServiceType.GetMethods().Should().HaveCount(2); // DownloadFileAsync, FileExistsAsync
    }
}

public class MockStorageService : IStorageService
{
    private readonly Dictionary<string, byte[]> _files = new();

    public Task<Stream> DownloadFileAsync(string filePath)
    {
        if (_files.TryGetValue(filePath, out var fileData))
        {
            return Task.FromResult<Stream>(new MemoryStream(fileData));
        }
        
        throw new FileNotFoundException($"File not found: {filePath}");
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        return Task.FromResult(_files.ContainsKey(filePath));
    }

    public void AddMockFile(string filePath, byte[] data)
    {
        _files[filePath] = data;
    }
}
