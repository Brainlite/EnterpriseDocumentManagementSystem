using EnterpriseDocumentManagementSystem.Api.Controllers;
using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EnterpriseDocumentManagementSystem.Api.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<MockAuthService> _mockAuthServiceMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<AuthController>> _loggerMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
        configMock.Setup(c => c["Jwt:Secret"]).Returns("test-secret-key-at-least-32-characters-long-for-security");
        configMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
        configMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");
        
        var mapperMock = new Mock<AutoMapper.IMapper>();
        mapperMock.Setup(m => m.Map<UserPayload>(It.IsAny<User>()))
            .Returns((User user) => new UserPayload(user.Id, user.Email, user.Role, user.Name));
        
        _mockAuthServiceMock = new Mock<MockAuthService>(configMock.Object, mapperMock.Object);
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthServiceMock.Object, _loggerMock.Object);
    }

    // Note: Login tests are covered by MockAuthServiceTests
    // since MockAuthService methods are not virtual and cannot be mocked

    [Fact]
    public void GetCurrentUser_AuthenticatedUser_ShouldReturnUserInfo()
    {
        // Arrange
        var userPayload = new UserPayload(
            Sub: "1",
            Email: "admin@example.com",
            Role: Roles.Admin,
            Name: "Admin User"
        );

        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUser"] = userPayload;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        // Act
        var result = _controller.GetCurrentUser();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var user = okResult.Value.Should().BeOfType<UserPayload>().Subject;
        user.Sub.Should().Be("1");
        user.Email.Should().Be("admin@example.com");
        user.Name.Should().Be("Admin User");
        user.Role.Should().Be(Roles.Admin);
    }

    [Fact]
    public void Logout_ShouldReturnOk()
    {
        // Act
        var result = _controller.Logout();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().NotBeNull();
    }

    // Note: GetUsers test is covered by MockAuthServiceTests
    // since MockAuthService.GetAllMockUsers() is not virtual and cannot be mocked
}
