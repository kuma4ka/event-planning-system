using EventPlanning.Application.DTOs.Venue;
using FluentValidation;

namespace EventPlanning.Application.Validators.Venue;

public class CreateVenueDtoValidator : VenueBaseDtoValidator<CreateVenueDto>
{
    public CreateVenueDtoValidator()
    {
        RuleFor(x => x.ImageFile)
            .NotNull().WithMessage("Image is required when creating a venue.");
    }
}