using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;
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
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        await context.Database.MigrateAsync();

        await EnsureRoleAsync(roleManager, "Admin");
        await EnsureRoleAsync(roleManager, "User");

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            throw new InvalidOperationException(
                "Seeding failed: Admin credentials are missing. " +
                "Use 'dotnet user-secrets set' or Environment Variables to set 'Seed:AdminEmail' and 'Seed:AdminPassword'.");

        var adminUser = await EnsureUserAsync(userManager, adminEmail, adminPassword, "System", "Admin", "Admin");

        var organizerUser =
            await EnsureUserAsync(userManager, "organizer@example.com", "Password123!", "John", "Doe", "User");

        if (await context.Venues.AnyAsync()) return;

        var venues = new List<Venue>
        {
            // USA
            new()
            {
                Name = "Madison Square Garden",
                Address = "4 Pennsylvania Plaza, New York, NY 10001, USA",
                Capacity = 20789,
                Description = "The World's Most Famous Arena. A multi-purpose indoor arena in New York City.",
                OrganizerId = adminUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1588196406432-0433f6ed5927?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Moscone Center",
                Address = "747 Howard St, San Francisco, CA 94103, USA",
                Capacity = 5000,
                Description = "The largest convention and exhibition complex in San Francisco.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1639527027808-7d178a943028?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Red Rocks Amphitheatre",
                Address = "18300 W Alameda Pkwy, Morrison, CO 80465, USA",
                Capacity = 9525,
                Description = "An open-air amphitheatre built into a rock structure in the western United States.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1593621198039-c87c6f91cbb1?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            // Europe
            new()
            {
                Name = "The Royal Albert Hall",
                Address = "Kensington Gore, South Kensington, London SW7 2AP, UK",
                Capacity = 5272,
                Description = "A concert hall on the northern edge of South Kensington, London.",
                OrganizerId = adminUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1655769121160-253b1b01888c?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Station F",
                Address = "5 Parvis Alan Turing, 75013 Paris, France",
                Capacity = 3000,
                Description = "The world's largest startup facility, located in the 13th arrondissement of Paris.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1632042856663-90ba0e32bf7a?q=80&w=1176&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Elbphilharmonie",
                Address = "Platz d. Deutschen Einheit 4, 20457 Hamburg, Germany",
                Capacity = 2100,
                Description = "One of the largest and most acoustically advanced concert halls in the world.",
                OrganizerId = adminUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1553547274-0df401ae03c9?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            // Ukraine
            new()
            {
                Name = "NSC Olimpiyskiy",
                Address = "Velyka Vasylkivska St, 55, Kyiv, Ukraine, 02000",
                Capacity = 70050,
                Description = "The premier sports and entertainment venue in Ukraine.",
                OrganizerId = adminUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1636241502039-3b8e908f7197?q=80&w=1332&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Unit.City",
                Address = "Dorohozhytska St, 3, Kyiv, Ukraine, 04119",
                Capacity = 1000,
                Description = "First innovation park in Ukraine. Perfect for tech conferences and hackathons.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1686902318140-c8ba4f3812b8?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Lviv Theatre of Opera and Ballet",
                Address = "Svobody Ave, 28, Lviv, Lviv Oblast, Ukraine, 79000",
                Capacity = 1100,
                Description = "An architectural gem and one of the most beautiful opera houses in Europe.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1748466991647-725993f162eb?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            // Asia / Australia
            new()
            {
                Name = "Sydney Opera House",
                Address = "Bennelong Point, Sydney NSW 2000, Australia",
                Capacity = 5738,
                Description = "A multi-venue performing arts centre at Sydney Harbour.",
                OrganizerId = adminUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1523059623039-a9ed027e7fad?q=80&w=1132&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            },
            new()
            {
                Name = "Marina Bay Sands Expo",
                Address = "10 Bayfront Ave, Singapore 018956",
                Capacity = 45000,
                Description = "Asia's leading destination for business, leisure and entertainment.",
                OrganizerId = organizerUser.Id,
                ImageUrl =
                    "https://images.unsplash.com/photo-1727549150616-21b6853f2fb2?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"
            }
        };

        await context.Venues.AddRangeAsync(venues);
        await context.SaveChangesAsync();

        var events = new List<Event>
        {
            new("Global Tech Summit 2024", "Annual gathering of tech leaders and innovators.", DateTime.Now.AddMonths(2), EventType.Conference, organizerUser.Id, venues[1].Id, false),
            new("Rock Legends Live", "A night of classic rock hits under the stars.", DateTime.Now.AddDays(14), EventType.Concert, adminUser.Id, venues[2].Id, false),
            new("Kyiv Startup Day", "Networking and pitch sessions for UA startups.", DateTime.Now.AddDays(5), EventType.Workshop, organizerUser.Id, venues[7].Id, false),
            new("Classic Evening", "Mozart and Bach performed by the Symphony Orchestra.", DateTime.Now.AddMonths(1), EventType.Concert, adminUser.Id, venues[5].Id, false),
            new("Past Event Example", "This event already happened.", DateTime.Now.AddMonths(-1), EventType.NetworkingEvent, organizerUser.Id, venues[0].Id, false)
        };

        await context.Events.AddRangeAsync(events);
        await context.SaveChangesAsync();
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName)) await roleManager.CreateAsync(new IdentityRole(roleName));
    }

    private static async Task<User> EnsureUserAsync(UserManager<User> userManager, string email, string password,
        string fName, string lName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                FirstName = fName,
                LastName = lName,
                Role = Enum.Parse<UserRole>(role),
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new Exception(
                    $"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(user, role)) await userManager.AddToRoleAsync(user, role);

        return user;
    }
}