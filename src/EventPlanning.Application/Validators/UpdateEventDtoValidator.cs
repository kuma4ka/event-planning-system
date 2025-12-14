using EventPlanning.Application.DTOs;
using FluentValidation;

namespace EventPlanning.Application.Validators;

public class UpdateEventDtoValidator : AbstractValidator<UpdateEventDto>
{
    public UpdateEventDtoValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Event name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500);

        RuleFor(x => x.Date)
            .GreaterThan(DateTime.Now).WithMessage("Event date cannot be in the past.");

        RuleFor(x => x.Type).IsInEnum();
    }
}