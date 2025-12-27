using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Extensions;

public static class SelectListItemExtensions
{
    public static IEnumerable<SelectListItem> ToSelectList<TEnum>(this TEnum? selectedValue, string defaultText = "All") 
        where TEnum : struct, Enum
    {
        var items = new List<SelectListItem>();

        if (!string.IsNullOrEmpty(defaultText))
        {
            items.Add(new SelectListItem
            {
                Value = "",
                Text = defaultText,
                Selected = !selectedValue.HasValue
            });
        }

        var enumItems = Enum.GetValues<TEnum>().Select(e => new SelectListItem
        {
            Value = e.ToString(),
            Text = e.GetDisplayName(),
            Selected = selectedValue.HasValue && e.Equals(selectedValue.Value)
        });

        items.AddRange(enumItems);

        return items;
    }
}