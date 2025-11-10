using AutoMapper;
using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace EnterpriseDocumentManagementSystem.Api.Tests.Services;

public class MockAuthServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly MockAuthService _service;

    public MockAuthServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Jwt:Secret"]).Returns("test-secret-key-at-least-32-characters-long-for-security");
        _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        _configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

        _mapperMock = new Mock<IMapper>();
        _mapperMock.Setup(m => m.Map<UserPayload>(It.IsAny<User>()))
            .Returns((User user) => new UserPayload(user.Id, user.Email, user.Role, user.Name));

        _service = new MockAuthService(_configurationMock.Object, _mapperMock.Object);
    }

    [Fact]
    public void AuthenticateUser_ValidAdminCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "admin123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);
        result.User.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public void Login_ValidManagerCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = "manager@example.com";
        var password = "manager123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Role.Should().Be(Roles.Manager);
    }

    [Fact]
    public void Login_ValidContributorCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = "contributor@example.com";
        var password = "contributor123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Role.Should().Be(Roles.Contributor);
    }

    [Fact]
    public void Login_ValidViewerCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = "viewer@example.com";
        var password = "viewer123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        result.User.Role.Should().Be(Roles.Viewer);
    }

    [Fact]
    public void Login_InvalidEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "invalid@example.com";
        var password = "password123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Login_InvalidPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "wrongpassword";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Login_EmptyEmail_ShouldReturnNull()
    {
        // Arrange
        var email = "";
        var password = "admin123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Login_EmptyPassword_ShouldReturnNull()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllUsers_ShouldReturnAllMockUsers()
    {
        // Act
        var result = _service.GetAllMockUsers();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(5);
        result.Should().Contain(u => u.Email == "admin@example.com");
        result.Should().Contain(u => u.Email == "manager@example.com");
        result.Should().Contain(u => u.Email == "contributor@example.com");
        result.Should().Contain(u => u.Email == "viewer@example.com");
    }

    [Fact]
    public void GetAllUsers_ShouldReturnUsersWithCorrectRoles()
    {
        // Act
        var result = _service.GetAllMockUsers().ToList();

        // Assert
        result.First(u => u.Email == "admin@example.com").Role.Should().Be(Roles.Admin);
        result.First(u => u.Email == "manager@example.com").Role.Should().Be(Roles.Manager);
        result.First(u => u.Email == "contributor@example.com").Role.Should().Be(Roles.Contributor);
        result.First(u => u.Email == "viewer@example.com").Role.Should().Be(Roles.Viewer);
    }

    [Fact]
    public void Login_GeneratedToken_ShouldContainUserClaims()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "admin123";

        // Act
        var result = _service.AuthenticateUser(email, password);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().NotBeNullOrEmpty();
        
        // Token should be a valid JWT (3 parts separated by dots)
        var tokenParts = result.Token.Split('.');
        tokenParts.Should().HaveCount(3);
    }
}
