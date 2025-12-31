using EventPlanning.Application.DTOs.Venue;
using FluentValidation;

namespace EventPlanning.Application.Validators.Venue;

public class UpdateVenueDtoValidator : VenueBaseDtoValidator<UpdateVenueDto>
{
    public UpdateVenueDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Invalid Venue ID.");
    }
}