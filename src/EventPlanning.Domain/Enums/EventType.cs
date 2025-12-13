using System.ComponentModel;

namespace EventPlanning.Domain.Enums;

public enum EventType
{
    [Description("Conference")] Conference,
    [Description("Seminar")] Seminar,
    [Description("Workshop")] Workshop,
    [Description("Networking Event")] NetworkingEvent,
    [Description("Concert")] Concert,
    [Description("Exhibition")] Exhibition,
    [Description("Webinar")] Webinar,
    [Description("Hackathon")] Hackathon,
    [Description("Gala Dinner")] GalaDinner,
    [Description("Charity Event")] CharityEvent,
    [Description("Sports Event")] SportsEvent,
    [Description("Product Launch")] ProductLaunch
}