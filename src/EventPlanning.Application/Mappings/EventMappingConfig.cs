using EventPlanning.Application.DTOs.Event;
using EventPlanning.Application.DTOs.Guest;
using EventPlanning.Domain.Entities;
using Mapster;

namespace EventPlanning.Application.Mappings;

public class EventMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Event, EventDetailsDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Date, src => src.Date)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.OrganizerId, src => src.OrganizerId)
            .Map(dest => dest.VenueName, src => src.Venue != null ? src.Venue.Name : "TBD")
            .Map(dest => dest.VenueId, src => src.VenueId)
            .Map(dest => dest.VenueImageUrl, src => src.Venue != null ? src.Venue.ImageUrl : null)
            .Map(dest => dest.VenueAddress, src => src.Venue != null ? src.Venue.Address : null)
            .Map(dest => dest.VenueCapacity, src => src.Venue != null ? src.Venue.Capacity : 0)
            .Map(dest => dest.IsPrivate, src => src.IsPrivate)
            .Map(dest => dest.Guests, src => src.Guests.Adapt<List<GuestDto>>())
            .Map(dest => dest.OrganizerName, src => "Unknown")
            .Map(dest => dest.OrganizerEmail, src => "");

        config.NewConfig<Event, EventDto>()
            .Map(dest => dest.VenueName, src => src.Venue != null ? src.Venue.Name : "TBD");

        config.NewConfig<Guest, GuestDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.CountryCode, src => src.CountryCode)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber != null ? src.PhoneNumber.Value : "");
    }
}
