using FluentValidation;
using EnterpriseDocumentManagementSystem.Api.Models.DTOs;
using EnterpriseDocumentManagementSystem.Api.Models.Entities;

namespace EnterpriseDocumentManagementSystem.Api.Validators;

public class DocumentUploadRequestValidator : AbstractValidator<DocumentUploadRequest>
{
    public DocumentUploadRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

        RuleFor(x => x.AccessType)
            .IsInEnum().WithMessage("Invalid access type");

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");
    }
}

public class DocumentUpdateRequestValidator : AbstractValidator<DocumentUpdateRequest>
{
    public DocumentUpdateRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(255).WithMessage("Title cannot exceed 255 characters")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description != null);

        RuleFor(x => x.AccessType)
            .IsInEnum().WithMessage("Invalid access type")
            .When(x => x.AccessType.HasValue);

        RuleForEach(x => x.Tags)
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters")
            .When(x => x.Tags != null);
    }
}

public class DocumentSearchRequestValidator : AbstractValidator<DocumentSearchRequest>
{
    public DocumentSearchRequestValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100).WithMessage("Page size cannot exceed 100");

        RuleFor(x => x.AccessType)
            .IsInEnum().WithMessage("Invalid access type")
            .When(x => x.AccessType.HasValue);
    }
}

public class ShareDocumentRequestValidator : AbstractValidator<ShareDocumentRequest>
{
    public ShareDocumentRequestValidator()
    {
        RuleFor(x => x.DocumentId)
            .NotEmpty().WithMessage("Document ID is required");

        RuleFor(x => x.SharedWithUserId)
            .NotEmpty().WithMessage("User ID is required")
            .MaximumLength(100).WithMessage("User ID cannot exceed 100 characters");

        RuleFor(x => x.PermissionLevel)
            .IsInEnum().WithMessage("Invalid permission level");

        RuleFor(x => x.ExpirationDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("Expiration date must be in the future")
            .When(x => x.ExpirationDate.HasValue);
    }
}

public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required")
            .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters");

        RuleFor(x => x.Color)
            .MaximumLength(100).WithMessage("Color cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Color));
    }
}
