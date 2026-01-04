using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventPlanning.Infrastructure.Persistence;

namespace EventPlanning.IntegrationTests.Controllers;

public class GuestControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = true
    });

    [Fact]
    public async Task AddGuestManually_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 1. Setup: Create Event
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();

        var evt = new Event("Guest Test Event", "Desc", DateTime.UtcNow.AddDays(10), EventType.Conference, organizer!.Id, venue!.Id);
        await context.Events.AddAsync(evt);
        await context.SaveChangesAsync();
        var eventId = evt.Id;

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Token (from Details page)
        var detailsUrl = $"/events/details/{eventId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Submit Add Guest
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["EventId"] = eventId.ToString(),
            ["FirstName"] = "John",
            ["LastName"] = "Manual",
            ["Email"] = "manual.guest@test.com",
            ["CountryCode"] = "+1",
            ["PhoneNumber"] = "1234567890",
            ["__RequestVerificationToken"] = token
        });

        var response = await _client.PostAsync("/guests/add-manually", content);
        response.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        response.RequestMessage?.RequestUri?.ToString().Should().Contain(detailsUrl);

        // 6. Verify DB
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var guest = await verifyContext.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Email == "manual.guest@test.com");
        guest.Should().NotBeNull();
        guest.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task EditGuest_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 1. Setup: Create Event and Guest
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();

        var evt = new Event("Edit Guest Event", "Desc", DateTime.UtcNow.AddDays(10), EventType.Conference, organizer!.Id, venue!.Id);
        await context.Events.AddAsync(evt);
        await context.SaveChangesAsync();

        var guest = new Guest("Jane", "Original", "jane@test.com", evt.Id, "+1", "9876543210");
        await context.Guests.AddAsync(guest);
        await context.SaveChangesAsync();

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Token
        var detailsUrl = $"/events/details/{evt.Id}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Submit Edit
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = guest.Id.ToString(),
            ["EventId"] = evt.Id.ToString(),
            ["FirstName"] = "Jane",
            ["LastName"] = "Edited",
            ["Email"] = "jane.edited@test.com",
            ["CountryCode"] = "+44",
            ["PhoneNumber"] = "7700900000",
            ["__RequestVerificationToken"] = token
        });

        var response = await _client.PostAsync("/guests/edit", content);
        response.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        response.RequestMessage?.RequestUri?.ToString().Should().Contain(detailsUrl);

        // 6. Verify DB
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedGuest = await verifyContext.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guest.Id);
        updatedGuest.Should().NotBeNull();
        updatedGuest.LastName.Should().Be("Edited");
        updatedGuest.Email.Value.Should().Be("jane.edited@test.com");
    }

    [Fact]
    public async Task RemoveGuest_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 1. Setup: Create Event and Guest
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();

        var evt = new Event("Remove Guest Event", "Desc", DateTime.UtcNow.AddDays(10), EventType.Conference, organizer!.Id, venue!.Id);
        await context.Events.AddAsync(evt);
        await context.SaveChangesAsync();

        var guest = new Guest("Bob", "Remove", "bob@test.com", evt.Id, "+1", "1231231234");
        await context.Guests.AddAsync(guest);
        await context.SaveChangesAsync();

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Token
        var detailsUrl = $"/events/details/{evt.Id}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Submit Remove
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["EventId"] = evt.Id.ToString(),
            ["GuestId"] = guest.Id.ToString(),
            ["__RequestVerificationToken"] = token
        });

        var response = await _client.PostAsync("/guests/remove", content);
        response.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        response.RequestMessage?.RequestUri?.ToString().Should().Contain(detailsUrl);

        // 6. Verify DB
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var removedGuest = await verifyContext.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == guest.Id);
        removedGuest.Should().BeNull();
    }
}
