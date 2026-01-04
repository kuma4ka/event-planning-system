using EventPlanning.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Persistence;

namespace EventPlanning.IntegrationTests.Controllers;

public class VenueControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = true
    });

    [Fact]
    public async Task CreateVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            if (admin == null) throw new Exception("Seeding failed: Admin not found.");

            _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        var createPageUrl = "/Admin/Venue/create";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, createPageUrl);

        var venueName = "Integration Test Venue";
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(venueName), "Name");
        content.Add(new StringContent("123 Test St, Test City"), "Address");
        content.Add(new StringContent("500"), "Capacity");
        content.Add(new StringContent("Created by Integration Test"), "Description");
        content.Add(new StringContent(token), "__RequestVerificationToken");
        
        // Mock Image File
        var fileContent = new ByteArrayContent([1, 2, 3, 4]);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        content.Add(fileContent, "ImageFile", "test-image.jpg");

        var createResponse = await _client.PostAsync("/Admin/Venue/create", content);
        createResponse.EnsureSuccessStatusCode();

        var finalUrl = createResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Name == venueName);
            venue.Should().NotBeNull();
            venue.Capacity.Should().Be(500);
        }
    }

    [Fact]
    public async Task Index_ShouldReturnVenues_WhenUserIsAdmin()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin!.IdentityId);
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        var response = await _client.GetAsync("/Admin/Venue");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Madison Square Garden"); // Seeded venue
    }

    [Fact]
    public async Task EditVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        
        var venue = new Domain.Entities.Venue(
            "Venue To Edit", "Original Address", 100, admin!.Id, 
            "Original Description");
        await context.Venues.AddAsync(venue);
        await context.SaveChangesAsync();
        var venueId = venue.Id;

        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var editPageUrl = $"/Admin/Venue/edit/{venueId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, editPageUrl);

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(venueId.ToString()), "Id");
        content.Add(new StringContent("Edited Venue Name"), "Name");
        content.Add(new StringContent("Edited Address"), "Address");
        content.Add(new StringContent("200"), "Capacity");
        content.Add(new StringContent("Edited Description"), "Description");
        content.Add(new StringContent(token), "__RequestVerificationToken");
        
        var editResponse = await _client.PostAsync(editPageUrl, content);
        editResponse.EnsureSuccessStatusCode();


        var finalUrl = editResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedVenue = await verifyContext.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == venueId);
        updatedVenue.Should().NotBeNull();
        updatedVenue.Name.Should().Be("Edited Venue Name");
        updatedVenue.Capacity.Should().Be(200);
    }

    [Fact]
    public async Task DeleteVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        
        var venue = new Domain.Entities.Venue(
            "Venue To Delete", "Delete Address", 100, admin!.Id, 
            "Delete Description");
        await context.Venues.AddAsync(venue);
        await context.SaveChangesAsync();
        var venueId = venue.Id;

        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        var indexUrl = "/Admin/Venue";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, indexUrl);

        var deleteUrl = $"/Admin/Venue/delete/{venueId}";
        var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        var deleteResponse = await _client.PostAsync(deleteUrl, deleteContent);
        deleteResponse.EnsureSuccessStatusCode();

        var finalUrl = deleteResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        using var verifyScope = factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedVenue = await verifyContext.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == venueId);
        deletedVenue.Should().BeNull();
    }
}
