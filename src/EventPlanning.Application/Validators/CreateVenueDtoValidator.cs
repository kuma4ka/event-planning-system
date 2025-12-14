using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class CreateVenueDtoValidator : AbstractValidator<CreateVenueDto>
{
    public CreateVenueDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        
        RuleFor(x => x.Address).NotEmpty().MaximumLength(200);
        
        RuleFor(x => x.Capacity).GreaterThan(0);
        
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}