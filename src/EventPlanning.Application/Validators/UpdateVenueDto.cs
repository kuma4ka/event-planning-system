using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class UpdateVenueDtoValidator : AbstractValidator<UpdateVenueDto>
{
    public UpdateVenueDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Venue name is required.")
            .MaximumLength(200);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200);

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.ImageUrl)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.ImageUrl))
            .WithMessage("Image URL must be a valid absolute URL (e.g., https://example.com/image.jpg).");
    }
}