using EnterpriseDocumentManagementSystem.Api.Services;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace EnterpriseDocumentManagementSystem.Api.Tests.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly Mock<ILogger<LocalFileStorageService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly LocalFileStorageService _service;
    private readonly string _testStoragePath;

    public LocalFileStorageServiceTests()
    {
        _loggerMock = new Mock<ILogger<LocalFileStorageService>>();
        _configurationMock = new Mock<IConfiguration>();

        _testStoragePath = Path.Combine(Path.GetTempPath(), "TestDocumentStorage", Guid.NewGuid().ToString());
        
        // Setup configuration values
        var pathSection = new Mock<IConfigurationSection>();
        pathSection.Setup(s => s.Value).Returns(_testStoragePath);
        
        var sizeSection = new Mock<IConfigurationSection>();
        sizeSection.Setup(s => s.Value).Returns("10485760");
        
        _configurationMock.Setup(c => c["FileStorage:Path"]).Returns(_testStoragePath);
        _configurationMock.Setup(c => c["FileStorage:MaxFileSizeBytes"]).Returns("10485760");
        _configurationMock.Setup(c => c.GetSection("FileStorage:Path")).Returns(pathSection.Object);
        _configurationMock.Setup(c => c.GetSection("FileStorage:MaxFileSizeBytes")).Returns(sizeSection.Object);

        _service = new LocalFileStorageService(_configurationMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData("application/pdf", true)]
    [InlineData("text/plain", true)]
    [InlineData("application/msword", true)]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document", true)]
    [InlineData("application/x-executable", false)]
    [InlineData("application/x-msdownload", false)]
    [InlineData("image/jpeg", false)]
    [InlineData("image/png", false)]
    public void IsAllowedFileType_ShouldValidateContentType(string contentType, bool expected)
    {
        // Act
        var result = _service.IsAllowedFileType(contentType);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetMaxFileSize_ShouldReturnConfiguredValue()
    {
        // Act
        var result = _service.GetMaxFileSize();

        // Assert
        result.Should().Be(10485760);
    }

    [Fact]
    public async Task SaveFileAsync_ShouldSaveFileAndReturnPath()
    {
        // Arrange
        var fileName = "test-document.pdf";
        var content = "Test file content"u8.ToArray();
        using var stream = new MemoryStream(content);

        // Act
        var result = await _service.SaveFileAsync(stream, fileName, "application/pdf");

        // Assert
        result.Success.Should().BeTrue();
        result.FilePath.Should().NotBeNullOrEmpty();
        result.FilePath.Should().EndWith(".pdf");
        
        var fullPath = Path.Combine(_testStoragePath, result.FilePath!);
        File.Exists(fullPath).Should().BeTrue();
        
        var savedContent = await File.ReadAllBytesAsync(fullPath);
        savedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task GetFileAsync_ExistingFile_ShouldReturnStream()
    {
        // Arrange
        var fileName = "test-document.txt";
        var content = "Test file content"u8.ToArray();
        using var saveStream = new MemoryStream(content);
        var uploadResult = await _service.SaveFileAsync(saveStream, fileName, "text/plain");

        // Act
        var result = await _service.GetFileAsync(uploadResult.FilePath!);

        // Assert
        result.Should().NotBeNull();
        using var ms = new MemoryStream();
        await result!.Value.FileStream.CopyToAsync(ms);
        result.Value.FileStream.Dispose(); // Dispose the stream to release file handle
        ms.ToArray().Should().BeEquivalentTo(content);
        result.Value.ContentType.Should().Be("text/plain");
    }

    [Fact]
    public async Task GetFileAsync_NonExistingFile_ShouldReturnNull()
    {
        // Arrange
        var nonExistingPath = "non-existing-file.pdf";

        // Act
        var result = await _service.GetFileAsync(nonExistingPath);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ShouldDeleteFile()
    {
        // Arrange
        var fileName = "test-document-to-delete.txt";
        var content = "Test file content"u8.ToArray();
        using var stream = new MemoryStream(content);
        var uploadResult = await _service.SaveFileAsync(stream, fileName, "text/plain");
        var fullPath = Path.Combine(_testStoragePath, uploadResult.FilePath!);

        // Act
        var result = await _service.DeleteFileAsync(uploadResult.FilePath!);

        // Assert
        result.Should().BeTrue();
        File.Exists(fullPath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingPath = "non-existing-file.pdf";

        // Act
        var result = await _service.DeleteFileAsync(nonExistingPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task FileExistsAsync_ExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var fileName = "test-document.txt";
        var content = "Test file content"u8.ToArray();
        using var stream = new MemoryStream(content);
        var uploadResult = await _service.SaveFileAsync(stream, fileName, "text/plain");

        // Act
        var result = await _service.FileExistsAsync(uploadResult.FilePath!);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_NonExistingFile_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingPath = "non-existing-file.pdf";

        // Act
        var result = await _service.FileExistsAsync(nonExistingPath);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        // Cleanup test storage directory
        if (Directory.Exists(_testStoragePath))
        {
            try
            {
                // Wait a bit for file handles to be released
                System.Threading.Thread.Sleep(100);
                Directory.Delete(_testStoragePath, true);
            }
            catch (IOException)
            {
                // Ignore cleanup errors in tests
            }
        }
    }
}
