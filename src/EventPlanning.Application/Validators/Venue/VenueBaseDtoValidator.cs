using EventPlanning.Application.Constants;
using EventPlanning.Application.DTOs.Venue;
using FluentValidation;

namespace EventPlanning.Application.Validators.Venue;

public abstract class VenueBaseDtoValidator<T> : AbstractValidator<T> where T : VenueBaseDto
{
    protected VenueBaseDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required.")
            .MaximumLength(ValidationConstants.MaxNameLength)
            .WithMessage($"Venue name cannot exceed {ValidationConstants.MaxNameLength} characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(ValidationConstants.MaxAddressLength)
            .WithMessage($"Address cannot exceed {ValidationConstants.MaxAddressLength} characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0.")
            .LessThanOrEqualTo(ValidationConstants.MaxCapacity)
            .WithMessage($"Capacity cannot exceed {ValidationConstants.MaxCapacity}.");

        RuleFor(x => x.Description)
            .MaximumLength(ValidationConstants.MaxDescriptionLength)
            .WithMessage($"Description cannot exceed {ValidationConstants.MaxDescriptionLength} characters.");

        RuleFor(x => x.ImageFile)
            .Must(file => file == null || file.Length <= ValidationConstants.MaxImageSizeBytes)
            .WithMessage($"Image size must be less than {ValidationConstants.MaxImageSizeInMb}MB.")
            .Must(file =>
                file == null || ValidationConstants.AllowedImageContentTypes.Contains(file.ContentType.ToLower()))
            .WithMessage("File must be a valid image (JPEG, PNG, WebP).");
    }
}