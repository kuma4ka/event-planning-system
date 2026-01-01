using FluentValidation;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EventPlanning.Web.Extensions;

public static class ModelStateExtensions
{
    public static void AddValidationErrors(this ModelStateDictionary state, ValidationException ex)
    {
        foreach (var error in ex.Errors)
        {
            state.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
