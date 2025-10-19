using FluentValidation;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Infrastructure.Validation;

/// <summary>
/// Validator for document upload requests
/// </summary>
public class UploadDocumentValidator : AbstractValidator<UploadDocumentDto>
{
    private static readonly string[] AllowedFileTypes = { ".pdf", ".doc", ".docx", ".txt" };
    private const long MaxFileSize = 50 * 1024 * 1024; // 50MB

    public UploadDocumentValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(255)
            .WithMessage("Title cannot exceed 255 characters");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty")
            .Must(file => file.Length <= MaxFileSize)
            .WithMessage($"File size cannot exceed {MaxFileSize / (1024 * 1024)}MB")
            .Must(HaveAllowedFileType)
            .WithMessage($"File type must be one of: {string.Join(", ", AllowedFileTypes)}");

        RuleForEach(x => x.TagIds)
            .GreaterThan(0)
            .WithMessage("Tag ID must be greater than 0");
    }

    private static bool HaveAllowedFileType(IFormFile file)
    {
        if (file == null) return false;
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return AllowedFileTypes.Contains(extension);
    }
}
