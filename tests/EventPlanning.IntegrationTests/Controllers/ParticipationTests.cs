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

        Guid eventId;
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
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


        
        var detailsUrl = $"/events/details/{eventId}";
        var detailsToken = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);


        var joinResponse = await _client.PostAsync($"/events/join/{eventId}", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = detailsToken
        }));


        joinResponse.EnsureSuccessStatusCode();
        var finalUrl = joinResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain($"/events/details/{eventId}");


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


            var guest = new EventPlanning.Domain.Entities.Guest(
                "Admin", "User", "admin@test.com", evt.Id, "+1", "5551234567", adminUser!.Id
            );
            await context.Guests.AddAsync(guest);
            await context.SaveChangesAsync();
        }


        using (var scope = _factory.Services.CreateScope())
        {
             var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
             var adminUser = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
             _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, adminUser!.IdentityId);
        }


        var detailsUrl = $"/events/details/{eventId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, detailsUrl);


        var leaveContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });


        var response = await _client.PostAsync($"/events/leave/{eventId}", leaveContent);
        response.EnsureSuccessStatusCode();


        var finalUrl = response.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain(detailsUrl);


        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var evt = await context.Events.Include(e => e.Guests).AsNoTracking().FirstOrDefaultAsync(e => e.Id == eventId);
            evt.Should().NotBeNull();
            evt!.Guests.Should().NotContain(g => g.Email.Value == "admin@test.com");
        }
    }
}
