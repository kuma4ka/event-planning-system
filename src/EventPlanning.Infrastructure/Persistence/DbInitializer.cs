using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventPlanning.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        if (await context.Venues.AnyAsync()) return;

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException(
                "Seeding failed: Admin credentials are missing. " +
                "Use 'dotnet user-secrets set' or Environment Variables to set 'Seed:AdminEmail' and 'Seed:AdminPassword'.");
        }

        var systemUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (systemUser == null)
        {
            systemUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Admin",
                Role = UserRole.Admin,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(systemUser, adminPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create admin user: {errors}");
            }
        }

        var venues = new List<Venue>
        {
            new()
            {
                Name = "Grand Conference Hall",
                Address = "123 Main St, New York, NY",
                Capacity = 500,
                Description = "A large hall perfect for international conferences.",
                OrganizerId = systemUser.Id,
                ImageUrl = "https://images.unsplash.com/photo-1517457373958-b7bdd4587205"
            },
            new()
            {
                Name = "Cozy Coworking Space",
                Address = "45 Tech Park, San Francisco, CA",
                Capacity = 50,
                Description = "Modern space for workshops and small meetups.",
                OrganizerId = systemUser.Id,
                ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c"
            },
            new()
            {
                Name = "City Stadium",
                Address = "99 Sport Ave, Chicago, IL",
                Capacity = 10000,
                Description = "Huge open-air venue for concerts and sports.",
                OrganizerId = systemUser.Id,
                ImageUrl = "https://images.unsplash.com/photo-1516450360452-9312f5e86fc7"
            },
            new()
            {
                Name = "Rooftop Lounge",
                Address = "Sky Tower, Miami, FL",
                Capacity = 100,
                Description = "Luxury view for networking parties.",
                OrganizerId = systemUser.Id,
                ImageUrl = "https://images.unsplash.com/photo-1519671482502-9759101d4574"
            }
        };

        await context.Venues.AddRangeAsync(venues);
        await context.SaveChangesAsync();
    }
}