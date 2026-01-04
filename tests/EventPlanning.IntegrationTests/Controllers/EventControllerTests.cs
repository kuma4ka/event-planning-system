using EventPlanning.Domain.Enums;
using EventPlanning.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Persistence;

namespace EventPlanning.IntegrationTests.Controllers;

public class EventControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = true
    });

    [Fact]
    public async Task CreateEvent_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 0. Get a valid Venue ID from DB
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();
        if (venue == null) throw new Exception("Seeding failed: No venues found.");
            
        if (venue == null) throw new Exception("Seeding failed: No venues found.");
            
        // 1. Authenticate as Organizer (ID from seeded user?)
        // We need the Organizer's ID. Let's fetch it.
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        if (organizer == null) throw new Exception("Seeding failed: Organizer not found.");

        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Create Page
        var createPageUrl = "/events/create";
        var createToken = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, createPageUrl);

        // 4. Create Event
        var eventDate = DateTime.UtcNow.AddDays(10);
        var eventContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["CreateDto.Name"] = "Integration Test Event",
            ["CreateDto.Description"] = "Created via Integration Test",
            ["CreateDto.Date"] = eventDate.ToString("o"),
            ["CreateDto.Type"] = EventType.Conference.ToString(),
            ["CreateDto.VenueId"] = venue.Id.ToString(),
            ["__RequestVerificationToken"] = createToken
        });
            
        var createResponse = await _client.PostAsync("/events/create", eventContent);
        createResponse.EnsureSuccessStatusCode();

        // 5. Verify Redirect happened
        var finalUrl = createResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/events/my-events");
            
        // 6. Verify Event Exists in DB
        using (var checkScope = factory.Services.CreateScope())
        {
            var checkContext = checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var createdEvent = await checkContext.Events.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Name == "Integration Test Event");
            createdEvent.Should().NotBeNull();
                
            // 7. Verify Details Page Loads
            var detailsUrl = $"/events/details/{createdEvent!.Id}";
            var detailsResponse = await _client.GetAsync(detailsUrl);
            detailsResponse.EnsureSuccessStatusCode();
            var detailsHtml = await detailsResponse.Content.ReadAsStringAsync();
            detailsHtml.Should().Contain("Integration Test Event");
        }
    }

    [Fact]
    public async Task EditEvent_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 1. Setup: Create an event to edit
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();
        
        var eventId = Guid.NewGuid();
        var existingEvent = new EventPlanning.Domain.Entities.Event(
            "Event To Edit", "Original Description", DateTime.UtcNow.AddDays(20), 
            EventType.Workshop, organizer!.Id, venue!.Id, false
        );
        // We need to bypass the constructor's ID generation or set it if possible, 
        // but Entity usually generates it. Let's just add it and save to get ID.
        // Actually Event entity has protected set for Id? Let's check or just let EF handle it.
        // For simplicity let's rely on constructor or add and let EF generate.
        
        await context.Events.AddAsync(existingEvent);
        await context.SaveChangesAsync();
        eventId = existingEvent.Id; // Capture ID

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Edit Page
        var editPageUrl = $"/events/edit/{eventId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, editPageUrl);

        // 4. Submit Edit
        var newDate = DateTime.UtcNow.AddDays(25);
        var editContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["UpdateDto.Id"] = eventId.ToString(),
            ["UpdateDto.Name"] = "Edited Event Name",
            ["UpdateDto.Description"] = "Edited Description",
            ["UpdateDto.Date"] = newDate.ToString("o"),
            ["UpdateDto.Type"] = EventType.NetworkingEvent.ToString(),
            ["UpdateDto.VenueId"] = venue.Id.ToString(),
            ["__RequestVerificationToken"] = token
        });

        var editResponse = await _client.PostAsync(editPageUrl, editContent);
        editResponse.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        var finalUrl = editResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/events/my-events");

        // 6. Verify DB Update
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedEvent = await verifyContext.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        updatedEvent.Should().NotBeNull();
        updatedEvent!.Name.Should().Be("Edited Event Name");
        updatedEvent.Type.Should().Be(EventType.NetworkingEvent);
    }

    [Fact]
    public async Task DeleteEvent_ShouldSucceed_WhenUserIsOrganizer()
    {
        // 1. Setup: Create an event to delete
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
        var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();

        var eventToDelete = new EventPlanning.Domain.Entities.Event(
            "Event To Delete", "Delete Me", DateTime.UtcNow.AddDays(30), 
            EventType.Concert, organizer!.Id, venue!.Id, false
        );
        await context.Events.AddAsync(eventToDelete);
        await context.SaveChangesAsync();
        var eventId = eventToDelete.Id;

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, organizer.IdentityId);

        // 3. Get Token (from Details page or usually implicitly needed for POST)
        // Since Delete is a POST, we likely need a token. Usually Delete button is in My Events or Details.
        // Let's get it from Details page.
        var detailsUrl = $"/events/details/{eventId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Submit Delete
        var deleteUrl = $"/events/delete/{eventId}";
        var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        var deleteResponse = await _client.PostAsync(deleteUrl, deleteContent);
        deleteResponse.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        var finalUrl = deleteResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/events/my-events");

        // 6. Verify DB Deletion (Soft Delete or Hard Delete?)
        // Assuming Hard Delete based on standard controller code "eventService.DeleteEventAsync"
        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedEvent = await verifyContext.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
        deletedEvent.Should().BeNull();
    }
}
