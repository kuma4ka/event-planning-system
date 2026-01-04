using EventPlanning.IntegrationTests.Helpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using EventPlanning.Infrastructure.Persistence;

namespace EventPlanning.IntegrationTests.Controllers;

public class VenueControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public VenueControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    [Fact]
    public async Task CreateVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        // 1. Authenticate as Admin
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            if (admin == null) throw new Exception("Seeding failed: Admin not found.");

            _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        // 2. Get Create Page
        var createPageUrl = "/Admin/Venue/create";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, createPageUrl);

        // 3. Create Venue
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

        // 4. Verify Redirect to Index
        var finalUrl = createResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        // 5. Verify Venue Exists in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var venue = await context.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Name == venueName);
            venue.Should().NotBeNull();
            venue!.Capacity.Should().Be(500);
        }
    }

    [Fact]
    public async Task Index_ShouldReturnVenues_WhenUserIsAdmin()
    {
        // 1. Authenticate as Admin
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
            _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin!.IdentityId);
            _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");
        }

        // 2. Get Index
        var response = await _client.GetAsync("/Admin/Venue");
        response.EnsureSuccessStatusCode();

        // 3. Verify Content
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Madison Square Garden"); // Seeded venue
    }

    [Fact]
    public async Task EditVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        // 1. Setup: Create a venue to edit
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        
        var venue = new EventPlanning.Domain.Entities.Venue(
            "Venue To Edit", "Original Address", 100, admin!.Id, 
            "Original Description", null
        );
        await context.Venues.AddAsync(venue);
        await context.SaveChangesAsync();
        var venueId = venue.Id;

        // 2. Authenticate as Admin
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // 3. Get Edit Page and Token
        var editPageUrl = $"/Admin/Venue/edit/{venueId}";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, editPageUrl);

        // 4. Submit Edit
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(venueId.ToString()), "Id");
        content.Add(new StringContent("Edited Venue Name"), "Name");
        content.Add(new StringContent("Edited Address"), "Address");
        content.Add(new StringContent("200"), "Capacity");
        content.Add(new StringContent("Edited Description"), "Description");
        content.Add(new StringContent(token), "__RequestVerificationToken");
        
        // Optional: Send new image or null. Let's send null (no file part) or empty.
        // If we don't add "ImageFile", it comes as null, which is valid for update.

        var editResponse = await _client.PostAsync(editPageUrl, content);
        editResponse.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        var finalUrl = editResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        // 6. Verify DB Update
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedVenue = await verifyContext.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == venueId);
        updatedVenue.Should().NotBeNull();
        updatedVenue!.Name.Should().Be("Edited Venue Name");
        updatedVenue.Capacity.Should().Be(200);
    }

    [Fact]
    public async Task DeleteVenue_ShouldSucceed_WhenUserIsAdmin()
    {
        // 1. Setup: Create a venue to delete
        // Note: Venue must NOT have events, or delete will fail/throw based on service logic.
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var admin = await context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == "admin@test.com");
        
        var venue = new EventPlanning.Domain.Entities.Venue(
            "Venue To Delete", "Delete Address", 100, admin!.Id, 
            "Delete Description", null
        );
        await context.Venues.AddAsync(venue);
        await context.SaveChangesAsync();
        var venueId = venue.Id;

        // 2. Authenticate
        _client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeader, admin.IdentityId);
        _client.DefaultRequestHeaders.Add("X-Test-Role", "Admin");

        // 3. Get Token (e.g. from Index)
        var indexUrl = "/Admin/Venue";
        var token = await HtmlHelpers.GetAntiForgeryTokenAsync(_client, indexUrl);

        // 4. Submit Delete
        var deleteUrl = $"/Admin/Venue/delete/{venueId}";
        var deleteContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token
        });

        var deleteResponse = await _client.PostAsync(deleteUrl, deleteContent);
        deleteResponse.EnsureSuccessStatusCode();

        // 5. Verify Redirect
        var finalUrl = deleteResponse.RequestMessage?.RequestUri?.ToString();
        finalUrl.Should().Contain("/Admin/Venue");

        // 6. Verify DB Deletion
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var deletedVenue = await verifyContext.Venues.AsNoTracking().FirstOrDefaultAsync(v => v.Id == venueId);
        deletedVenue.Should().BeNull();
    }
}
