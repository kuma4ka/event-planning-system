using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using FluentAssertions;

namespace EventPlanning.Tests.Domain;

public class EventTests
{
    [Fact]
    public void Constructor_ShouldCreateEvent_WhenDataIsValid()
    {
        var name = "Test Event";
        var description = "Description";
        var date = DateTime.UtcNow.AddDays(1);
        var type = EventType.Conference;
        var organizerId = Guid.CreateVersion7();

        var evt = new Event(name, description, date, type, organizerId);

        evt.Name.Should().Be(name);
        evt.Description.Should().Be(description);
        evt.Date.Should().Be(date);
        evt.Type.Should().Be(type);
        evt.OrganizerId.Should().Be(organizerId);
        evt.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        evt.IsPrivate.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string? invalidName)
    {
        Action act = () => new Event(invalidName!, "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());

        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenDateIsInPast()
    {
        Action act = () => new Event("Name", "Desc", DateTime.UtcNow.AddDays(-1), EventType.Conference, Guid.CreateVersion7());

        act.Should().Throw<ArgumentException>().WithMessage("*past*");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFields_WhenDataIsValid()
    {
        var evt = new Event("Name", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());
        var newName = "New Name";
        var newDesc = "New Desc";
        var newDate = DateTime.UtcNow.AddDays(2);
        var newType = EventType.Workshop;
        var venueId = Guid.CreateVersion7();

        evt.UpdateDetails(newName, newDesc, newDate, newType, venueId);

        evt.Name.Should().Be(newName);
        evt.Description.Should().Be(newDesc);
        evt.Date.Should().Be(newDate);
        evt.Type.Should().Be(newType);
        evt.VenueId.Should().Be(venueId);
    }

    [Fact]
    public void UpdateDetails_ShouldThrowArgumentException_WhenNewDateIsPast()
    {
        var evt = new Event("Name", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());

        Action act = () => evt.UpdateDetails("Name", "Desc", DateTime.UtcNow.AddDays(-1), EventType.Conference, null);

        act.Should().Throw<ArgumentException>().WithMessage("*past*");
    }

    [Fact]
    public void CanAddGuest_ShouldNotThrow_WhenEventIsFutureAndHasCapacity()
    {
        var evt = new Event("Name", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());
        
        Action act = () => evt.CanAddGuest(100);

        act.Should().NotThrow();
    }

    [Fact]
    public void CanAddGuest_ShouldThrowInvalidOperation_WhenEventIsPast()
    {
        var evt = new Event("Name", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());
        typeof(Event).GetProperty(nameof(Event.Date))!.SetValue(evt, DateTime.UtcNow.AddDays(-1));

        Action act = () => evt.CanAddGuest(100);

        act.Should().Throw<InvalidOperationException>().WithMessage("*ended*");
    }

    [Fact]
    public void CanAddGuest_ShouldThrowInvalidOperation_WhenVenueReachedCapacity()
    {
        var evt = new Event("Name", "Desc", DateTime.UtcNow.AddDays(1), EventType.Conference, Guid.CreateVersion7());
        
        var venue = new Venue("Venue", "Addr", 1, Guid.CreateVersion7());
        typeof(Event).GetProperty(nameof(Event.Venue))!.SetValue(evt, venue);

        Action act = () => evt.CanAddGuest(1);

        act.Should().Throw<InvalidOperationException>().WithMessage("*fully booked*");
    }
}
