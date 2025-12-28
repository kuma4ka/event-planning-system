using EventPlanning.Application.DTOs.Event;
using FluentValidation;

namespace EventPlanning.Application.Validators.Event;

public class EventSearchDtoValidator : AbstractValidator<EventSearchDto>
{
    public EventSearchDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.SearchTerm)
            .MaximumLength(100)
            .WithMessage("Search term cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SearchTerm));

        RuleFor(x => x.FromDate)
            .GreaterThanOrEqualTo(new DateTime(2025, 1, 1))
            .WithMessage("Date is too far in the past.")
            .When(x => x.FromDate.HasValue);

        RuleFor(x => x.ToDate)
            .GreaterThanOrEqualTo(new DateTime(2025, 1, 1))
            .WithMessage("Date is too far in the past.")
            .When(x => x.ToDate.HasValue);

        RuleFor(x => x.ToDate)
            .Must((model, toDate) => toDate!.Value.Date >= model.FromDate!.Value.Date)
            .WithMessage("End date must be greater than or equal to start date.")
            .When(x => x.FromDate.HasValue && x.ToDate.HasValue);
    }
}