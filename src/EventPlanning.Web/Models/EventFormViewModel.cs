using EventPlanning.Application.DTOs.Event;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventPlanning.Web.Models;

public class EventFormViewModel
{
    public CreateEventDto? CreateDto { get; set; }
    public UpdateEventDto? UpdateDto { get; set; }
    public List<SelectListItem> Venues { get; set; } = [];

    public bool IsEditMode => UpdateDto != null;
    
    public static EventFormViewModel ForCreate(CreateEventDto? dto, List<SelectListItem> venues)
    {
        return new EventFormViewModel { CreateDto = dto ?? new CreateEventDto("", "", DateTime.Today.AddDays(1), Domain.Enums.EventType.Conference, Guid.Empty), Venues = venues };
    }

    public static EventFormViewModel ForEdit(UpdateEventDto dto, List<SelectListItem> venues)
    {
        return new EventFormViewModel { UpdateDto = dto, Venues = venues };
    }
}
