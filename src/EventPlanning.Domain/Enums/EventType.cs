using System.ComponentModel.DataAnnotations;

namespace EventPlanning.Domain.Enums;

public enum EventType
{
    Conference,
    Seminar,
    Workshop,
    
    [Display(Name = "Networking Event")] NetworkingEvent,

    Concert,
    Exhibition,
    Webinar,
    Hackathon,

    [Display(Name = "Gala Dinner")] GalaDinner,

    [Display(Name = "Charity Event")] CharityEvent,

    [Display(Name = "Sports Event")] SportsEvent,

    [Display(Name = "Product Launch")] ProductLaunch
}