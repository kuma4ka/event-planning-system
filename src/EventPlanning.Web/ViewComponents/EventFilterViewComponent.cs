using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using EventPlanning.Domain.Enums;
using EventPlanning.Web.Models.Shared;
using EventPlanning.Web.Extensions; // Assuming this is where ToSelectList is

namespace EventPlanning.Web.ViewComponents;

public class EventFilterViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(
        string controllerName,
        string actionName,
        string? searchTerm = null,
        EventType? type = null,
        DateTime? from = null,
        DateTime? to = null,
        string? sortOrder = null,
        string? viewType = null,
        bool isHeroOverlay = false,
        bool showSearch = false)
    {
        var typeOptions = type.ToSelectList("All Categories");

        var model = new EventFilterViewModel
        {
            SearchTerm = searchTerm,
            Type = type,
            From = from,
            To = to,
            TypeOptions = typeOptions,
            MinDate = DateTime.Now.ToString("yyyy-MM-dd"),
            HasFilters = !string.IsNullOrEmpty(searchTerm) || type.HasValue || from.HasValue || to.HasValue,
            ControllerName = controllerName,
            ActionName = actionName,
            ViewType = viewType,
            SortOrder = sortOrder,
            ShowSearch = showSearch,
            IsHeroOverlay = isHeroOverlay
        };

        return View(model);
    }
}
