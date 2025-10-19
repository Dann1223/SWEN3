using FluentValidation;
using PaperlessRESTAPI.Models.DTOs;

namespace PaperlessRESTAPI.Infrastructure.Validation;

/// <summary>
/// Validator for tag DTOs
/// </summary>
public class TagValidator : AbstractValidator<TagDto>
{
    public TagValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Tag name is required")
            .MaximumLength(100)
            .WithMessage("Tag name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
            .WithMessage("Tag name can only contain letters, numbers, spaces, hyphens, and underscores");

        RuleFor(x => x.Description)
            .MaximumLength(255)
            .When(x => !string.IsNullOrEmpty(x.Description))
            .WithMessage("Tag description cannot exceed 255 characters");

        RuleFor(x => x.Color)
            .NotEmpty()
            .WithMessage("Color is required")
            .Matches(@"^#[0-9A-Fa-f]{6}$")
            .WithMessage("Color must be a valid hex color code (e.g., #007bff)");
    }
}
