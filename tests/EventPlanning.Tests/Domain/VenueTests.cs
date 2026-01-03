using EventPlanning.Domain.Entities;
using FluentAssertions;

namespace EventPlanning.Tests.Domain;

public class VenueTests
{
    [Fact]
    public void Constructor_ShouldCreateVenue_WhenDataIsValid()
    {

        var name = "Grand Hall";
        var address = "123 Main St";
        var capacity = 100;
        var adminId = Guid.CreateVersion7();


        var venue = new Venue(name, address, capacity, adminId);


        venue.Name.Should().Be(name);
        venue.Address.Should().Be(address);
        venue.Capacity.Should().Be(capacity);
        venue.OrganizerId.Should().Be(adminId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenNameIsInvalid(string? invalidName)
    {

        Action act = () => new Venue(invalidName!, "Address", -1, Guid.CreateVersion7());


        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenCapacityIsNegative()
    {

        Action act = () => new Venue("Name", "Address", -1, Guid.CreateVersion7());


        act.Should().Throw<ArgumentException>().WithMessage("*Capacity*");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFields_WhenDataIsValid()
    {

        var venue = new Venue("Old Name", "Old Address", 50, Guid.CreateVersion7());


        venue.UpdateDetails("New Name", "New Address", 200, "New Desc", null);


        venue.Name.Should().Be("New Name");
        venue.Address.Should().Be("New Address");
        venue.Capacity.Should().Be(200);
        venue.Description.Should().Be("New Desc");
    }
}
