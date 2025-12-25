using EventPlanning.Application.DTOs.Event;
using FluentValidation;

namespace EventPlanning.Application.Validators.Event;

public class EventSearchDtoValidator : AbstractValidator<EventSearchDto>
{
    public EventSearchDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x)
            .Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue || x.FromDate <= x.ToDate)
            .WithMessage("'From' date must be earlier than or equal to 'To' date.");
    }
}