using EventPlanning.Application.DTOs.Guest;
using FluentValidation;

namespace EventPlanning.Application.Validators.Guest;

public class UpdateGuestDtoValidator : GuestBaseDtoValidator<UpdateGuestDto>
{
    public UpdateGuestDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Guest ID is required for update.");
    }
}