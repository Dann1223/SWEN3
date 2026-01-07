using FluentAssertions;
using PaperlessRESTAPI.Services.Implementations;
using PaperlessRESTAPI.Services.Interfaces;

namespace PaperlessRESTAPI.Tests.Services;

/// <summary>
/// Unit tests for MinIOService - simplified to test interface compliance
/// </summary>
public class MinIOServiceTests
{
    [Fact]
    public void MinIOService_ShouldImplementIStorageService()
    {
        // Arrange & Act
        var serviceType = typeof(MinIOService);
        var interfaceType = typeof(IStorageService);

        // Assert
        interfaceType.IsAssignableFrom(serviceType).Should().BeTrue();
    }

    [Fact]
    public void MinIOService_ShouldHaveRequiredMethods()
    {
        // Arrange
        var serviceType = typeof(MinIOService);

        // Act & Assert
        serviceType.GetMethod(nameof(IStorageService.UploadFileAsync)).Should().NotBeNull();
        serviceType.GetMethod(nameof(IStorageService.DownloadFileAsync)).Should().NotBeNull();
        serviceType.GetMethod(nameof(IStorageService.DeleteFileAsync)).Should().NotBeNull();
        serviceType.GetMethod(nameof(IStorageService.FileExistsAsync)).Should().NotBeNull();
        serviceType.GetMethod(nameof(IStorageService.GetFileUrlAsync)).Should().NotBeNull();
    }

    [Fact]
    public void MinIOService_Methods_ShouldReturnCorrectTypes()
    {
        // Arrange
        var serviceType = typeof(MinIOService);

        // Act & Assert
        var uploadMethod = serviceType.GetMethod(nameof(IStorageService.UploadFileAsync));
        uploadMethod?.ReturnType.Should().Be(typeof(Task<string>));

        var downloadMethod = serviceType.GetMethod(nameof(IStorageService.DownloadFileAsync));
        downloadMethod?.ReturnType.Should().Be(typeof(Task<Stream?>));

        var deleteMethod = serviceType.GetMethod(nameof(IStorageService.DeleteFileAsync));
        deleteMethod?.ReturnType.Should().Be(typeof(Task));

        var existsMethod = serviceType.GetMethod(nameof(IStorageService.FileExistsAsync));
        existsMethod?.ReturnType.Should().Be(typeof(Task<bool>));

        var urlMethod = serviceType.GetMethod(nameof(IStorageService.GetFileUrlAsync));
        urlMethod?.ReturnType.Should().Be(typeof(Task<string?>));
    }
}
