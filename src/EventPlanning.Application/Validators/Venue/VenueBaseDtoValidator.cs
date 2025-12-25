using EventPlanning.Application.DTOs.Venue;
using FluentValidation;

namespace EventPlanning.Application.Validators.Venue;

public abstract class VenueBaseDtoValidator<T> : AbstractValidator<T> where T : VenueBaseDto
{
    protected VenueBaseDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required.")
            .MaximumLength(200).WithMessage("Venue name cannot exceed 200 characters.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ImageFile)
            .Must(file => file == null || file.Length <= 5 * 1024 * 1024)
            .WithMessage("Image size must be less than 5MB.")
            .Must(file => file == null || file.ContentType.StartsWith("image/"))
            .WithMessage("File must be a valid image.");
    }
}