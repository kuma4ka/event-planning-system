using EventPlanning.Domain.Entities;
using FluentAssertions;

namespace EventPlanning.Tests.Domain;

public class GuestTests
{
    [Fact]
    public void Constructor_ShouldCreateGuest_WhenDataIsValid()
    {
        var first = "John";
        var last = "Doe";
        var email = "john@example.com";
        var eventId = Guid.CreateVersion7();
        var countryCode = "+1";
        var phone = "1234567890";

        var guest = new Guest(first, last, email, eventId, countryCode, phone);

        guest.FirstName.Should().Be(first);
        guest.LastName.Should().Be(last);
        guest.Email.Value.Should().Be(email);
        guest.EventId.Should().Be(eventId);
        guest.CountryCode.Should().Be(countryCode);
        guest.PhoneNumber!.Value.Should().Be(phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowArgumentException_WhenFirstNameIsInvalid(string? invalidName)
    {
        Action act = () => new Guest(invalidName!, "Doe", "test@test.com", Guid.CreateVersion7(), "+1", "1234567");
        act.Should().Throw<ArgumentException>().WithMessage("*First Name*");
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenEmailIsInvalid()
    {
        Action act = () => new Guest("John", "Doe", "invalid-email", Guid.CreateVersion7(), "+1", "1234567");
        act.Should().Throw<ArgumentException>().WithMessage("*Email*");
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFields_WhenDataIsValid()
    {
        var guest = new Guest("Old", "Name", "old@test.com", Guid.CreateVersion7(), "+1", "1234567");

        guest.UpdateDetails("New", "Person", "new@test.com", "+44", "7654321");

        guest.FirstName.Should().Be("New");
        guest.LastName.Should().Be("Person");
        guest.Email.Value.Should().Be("new@test.com");
        guest.CountryCode.Should().Be("+44");
        guest.PhoneNumber!.Value.Should().Be("7654321");
    }
}