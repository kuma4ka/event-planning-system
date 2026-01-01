using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs.Guest;
using FluentValidation;

namespace EventPlanning.Application.Validators.Guest;

public class UpdateGuestDtoValidator : GuestBaseDtoValidator<UpdateGuestDto>
{
    public UpdateGuestDtoValidator(ICountryService countryService) : base(countryService)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Guest ID is required for update.");
    }
}