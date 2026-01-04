using EventPlanning.IntegrationTests.Helpers;
using EventPlanning.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EventPlanning.IntegrationTests.Controllers;

public class ParticipationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ParticipationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    [Fact]
    public async Task JoinEvent_ShouldSucceed_WhenUserIsAuthenticated()
    {
        // 0. Get a seeded event from DB
        Guid eventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // "Global Tech Summit 2024" is seeded for organizerUser, Admin can join.
            var evt = await context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Name == "Global Tech Summit 2024");
            if (evt == null) throw new Exception("Seeding failed: Event not found.");
            eventId = evt.Id;
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
             var adminUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
             if (adminUser == null) throw new Exception("Seeding failed: Admin not found.");
             _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, adminUser.IdentityId);
        }

        // 3. Get Details Page to get NEW Token (on details page, there is Join form?)
        // Join form is at /events/details/{id} usually.
        // Wait, EventParticipationController: [HttpPost("join/{id}")]
        // Does Details page have a form posting to it? Yes likely.
        // I need a CSRF token. I can get it from Details page.
        
        var detailsUrl = $"/events/details/{eventId}";
        var detailsToken = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Join Event
        var joinResponse = await _client.PostAsync($"/events/join/{eventId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = detailsToken
        }));

        // 5. Verify Redirect
        joinResponse.EnsureSuccessStatusCode();
        var finalUrl = joinResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain($"/events/details/{eventId}");

        // 6. Verify Participation in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var evt = await context.Events.Include(e => e.Guests).AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            evt.Should().NotBeNull();
            evt!.Guests.Should().Contain(g => g.Email == "admin@test.com"); // Assuming Guest email matches user
        }
    }

    [Fact]
    public async Task LeaveEvent_ShouldSucceed_WhenUserIsJoined()
    {
        // 1. Setup: Create Event and make Admin join it
        Guid eventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var organizer = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "organizer@test.com");
            var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync();
            var adminUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");

            var evt = new EventPlanning.Domain.Entities.Event(
                "Event To Leave", "Description", DateTime.UtcNow.AddDays(20), 
                EventPlanning.Domain.Enums.EventType.NetworkingEvent, organizer!.Id, venue!.Id, false
            );
            await context.Events.AddAsync(evt);
            await context.SaveChangesAsync();
            eventId = evt.Id;

            // Add Admin as Guest
            // Note: Guest constructor with UserId
            var guest = new EventPlanning.Domain.Entities.Guest(
                "Admin", "User", "admin@test.com", evt.Id, "+1", "5551234567", adminUser!.Id
            );
            await context.Guests.AddAsync(guest);
            await context.SaveChangesAsync();
        }

        // 2. Authenticate as Admin
        using (var scope = _factory.Services.CreateScope())
        {
             var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
             var adminUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
             _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, adminUser!.IdentityId);
        }

        // 3. Get Token
        var detailsUrl = $"/events/details/{eventId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);

        // 4. Submit Leave
        var leaveContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        // 5. POST to Leave endpoint
        var response = await _client.PostAsync($"/events/leave/{eventId}", leaveContent);
        response.EnsureSuccessStatusCode();

        // 6. Verify Redirect
        var finalUrl = response.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain(detailsUrl);

        // 7. Verify DB: User is NO LONGER in Guests
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var evt = await context.Events.Include(e => e.Guests).AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            evt.Should().NotBeNull();
            evt!.Guests.Should().NotContain(g => g.Email.Value == "admin@test.com");
        }
    }
}
