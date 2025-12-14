using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class CreateGuestDtoValidator : AbstractValidator<CreateGuestDto>
{
    public CreateGuestDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
        
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        
        RuleFor(x => x.EventId).NotEmpty();
    }
}