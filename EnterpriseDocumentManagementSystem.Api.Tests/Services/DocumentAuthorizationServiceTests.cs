using EnterpriseDocumentManagementSystem.Api.Models;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;
using EnterpriseDocumentManagementSystem.Api.Services;
using FluentAssertions;
using Xunit;

namespace EnterpriseDocumentManagementSystem.Api.Tests.Services;

public class DocumentAuthorizationServiceTests
{
    private readonly DocumentAuthorizationService _service;

    public DocumentAuthorizationServiceTests()
    {
        _service = new DocumentAuthorizationService();
    }

    [Theory]
    [InlineData(Roles.Admin, true)]
    [InlineData(Roles.Manager, true)]
    [InlineData(Roles.Contributor, true)]
    [InlineData(Roles.Viewer, false)]
    public void CanCreateDocuments_ShouldReturnCorrectPermission(string role, bool expected)
    {
        // Act
        var result = _service.CanCreateDocuments(role);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CanViewDocument_PublicDocument_ShouldAllowAllUsers()
    {
        // Arrange
        var document = new Document
        {
            AccessType = AccessType.Public,
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanViewDocument(Roles.Viewer, "anyuser123", document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanViewDocument_PrivateDocument_Owner_ShouldAllow()
    {
        // Arrange
        var userId = "user123";
        var document = new Document
        {
            AccessType = AccessType.Private,
            UploadedBy = userId
        };

        // Act
        var result = _service.CanViewDocument(Roles.Contributor, userId, document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanViewDocument_PrivateDocument_NonOwner_ShouldDeny()
    {
        // Arrange
        var document = new Document
        {
            AccessType = AccessType.Private,
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanViewDocument(Roles.Contributor, "otheruser123", document);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanViewDocument_PrivateDocument_Admin_ShouldAllow()
    {
        // Arrange
        var document = new Document
        {
            AccessType = AccessType.Private,
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanViewDocument(Roles.Admin, "admin123", document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEditDocument_Owner_ShouldAllow()
    {
        // Arrange
        var userId = "user123";
        var document = new Document
        {
            UploadedBy = userId
        };

        // Act
        var result = _service.CanEditDocument(Roles.Contributor, userId, document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanEditDocument_NonOwner_ShouldDeny()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanEditDocument(Roles.Contributor, "otheruser123", document);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanEditDocument_Admin_ShouldAllow()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanEditDocument(Roles.Admin, "admin123", document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanDeleteDocument_Owner_ShouldAllow()
    {
        // Arrange
        var userId = "user123";
        var document = new Document
        {
            UploadedBy = userId
        };

        // Act
        var result = _service.CanDeleteDocument(Roles.Contributor, userId, document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanDeleteDocument_NonOwner_ShouldDeny()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanDeleteDocument(Roles.Contributor, "otheruser123", document);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanDeleteDocument_Admin_ShouldAllow()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanDeleteDocument(Roles.Admin, "admin123", document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanShareDocument_Owner_ShouldAllow()
    {
        // Arrange
        var userId = "user123";
        var document = new Document
        {
            UploadedBy = userId
        };

        // Act
        var result = _service.CanShareDocument(Roles.Contributor, userId, document);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanShareDocument_NonOwner_ShouldDeny()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanShareDocument(Roles.Contributor, "otheruser123", document);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanShareDocument_Admin_ShouldAllow()
    {
        // Arrange
        var document = new Document
        {
            UploadedBy = "owner123"
        };

        // Act
        var result = _service.CanShareDocument(Roles.Admin, "admin123", document);

        // Assert
        result.Should().BeTrue();
    }
}
