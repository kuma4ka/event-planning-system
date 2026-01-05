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
    public static async Task SeedAsync(IServiceProvider serviceProvider, bool isDevelopment)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
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

        var adminUser = await EnsureUserAsync(context, userManager, adminEmail, adminPassword, "System", "Admin", "Admin", "+15550000001");

        var organizerEmail = configuration["Seed:OrganizerEmail"];
        var organizerPassword = configuration["Seed:OrganizerPassword"];

        if (string.IsNullOrEmpty(organizerEmail) || string.IsNullOrEmpty(organizerPassword))
            throw new InvalidOperationException(
                "Seeding failed: Organizer credentials are missing. " +
                "Use 'dotnet user-secrets set' or Environment Variables to set 'Seed:OrganizerEmail' and 'Seed:OrganizerPassword'.");

        var organizerUser =
            await EnsureUserAsync(context, userManager, organizerEmail, organizerPassword, "John", "Doe", "User", "+15550000002");

        if (isDevelopment)
        {
            IList<Venue> venues;
            if (!await context.Venues.AnyAsync())
            {
                venues = new List<Venue>
                {
                    // USA
                    new("Madison Square Garden", "4 Pennsylvania Plaza, New York, NY 10001, USA", 20789, adminUser.Id,
                        "The World's Most Famous Arena. A multi-purpose indoor arena in New York City.",
                        "https://images.unsplash.com/photo-1588196406432-0433f6ed5927?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Moscone Center", "747 Howard St, San Francisco, CA 94103, USA", 5000, organizerUser.Id,
                        "The largest convention and exhibition complex in San Francisco.",
                        "https://images.unsplash.com/photo-1639527027808-7d178a943028?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Red Rocks Amphitheatre", "18300 W Alameda Pkwy, Morrison, CO 80465, USA", 9525, organizerUser.Id,
                        "An open-air amphitheatre built into a rock structure in the western United States.",
                        "https://images.unsplash.com/photo-1593621198039-c87c6f91cbb1?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    // Europe
                    new("The Royal Albert Hall", "Kensington Gore, South Kensington, London SW7 2AP, UK", 5272,
                        adminUser.Id, "A concert hall on the northern edge of South Kensington, London.",
                        "https://images.unsplash.com/photo-1655769121160-253b1b01888c?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Station F", "5 Parvis Alan Turing, 75013 Paris, France", 3000, organizerUser.Id,
                        "The world's largest startup facility, located in the 13th arrondissement of Paris.",
                        "https://images.unsplash.com/photo-1632042856663-90ba0e32bf7a?q=80&w=1176&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Elbphilharmonie", "Platz d. Deutschen Einheit 4, 20457 Hamburg, Germany", 2100, adminUser.Id,
                        "One of the largest and most acoustically advanced concert halls in the world.",
                        "https://images.unsplash.com/photo-1553547274-0df401ae03c9?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    // Ukraine
                    new("NSC Olimpiyskiy", "Velyka Vasylkivska St, 55, Kyiv, Ukraine, 02000", 70050, adminUser.Id,
                        "The premier sports and entertainment venue in Ukraine.",
                        "https://images.unsplash.com/photo-1636241502039-3b8e908f7197?q=80&w=1332&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Unit.City", "Dorohozhytska St, 3, Kyiv, Ukraine, 04119", 1000, organizerUser.Id,
                        "First innovation park in Ukraine. Perfect for tech conferences and hackathons.",
                        "https://images.unsplash.com/photo-1686902318140-c8ba4f3812b8?q=80&w=1074&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Lviv Theatre of Opera and Ballet", "Svobody Ave, 28, Lviv, Lviv Oblast, Ukraine, 79000", 1100,
                        organizerUser.Id, "An architectural gem and one of the most beautiful opera houses in Europe.",
                        "https://images.unsplash.com/photo-1748466991647-725993f162eb?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    // Asia / Australia
                    new("Sydney Opera House", "Bennelong Point, Sydney NSW 2000, Australia", 5738, adminUser.Id,
                        "A multi-venue performing arts centre at Sydney Harbour.",
                        "https://images.unsplash.com/photo-1523059623039-a9ed027e7fad?q=80&w=1132&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D"),
                    new("Marina Bay Sands Expo", "10 Bayfront Ave, Singapore 018956", 45000, organizerUser.Id,
                        "Asia's leading destination for business, leisure and entertainment.",
                        "https://images.unsplash.com/photo-1727549150616-21b6853f2fb2?q=80&w=1170&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D")
                };
                await context.Venues.AddRangeAsync(venues);
                await context.SaveChangesAsync();
            }
            else
            {
                venues = await context.Venues.ToListAsync();
            }

            if (!await context.Events.AnyAsync())
            {
                Venue GetVenue(string namePart)
                {
                    return venues.First(v => v.Name.Contains(namePart));
                }

                var events = new List<Event>
                {
                    new("Global Tech Summit 2024", "Annual gathering of tech leaders and innovators.",
                        DateTime.UtcNow.AddMonths(2), EventType.Conference, organizerUser.Id, GetVenue("Moscone").Id,
                        false),
                    new("Rock Legends Live", "A night of classic rock hits under the stars.", DateTime.UtcNow.AddDays(14),
                        EventType.Concert, adminUser.Id, GetVenue("Red Rocks").Id, false),
                    new("Kyiv Startup Day", "Networking and pitch sessions for UA startups.", DateTime.UtcNow.AddDays(5),
                        EventType.Workshop, organizerUser.Id, GetVenue("Unit.City").Id, false),
                    new("Classic Evening", "Mozart and Bach performed by the Symphony Orchestra.",
                        DateTime.UtcNow.AddMonths(1), EventType.Concert, adminUser.Id, GetVenue("Elbphilharmonie").Id,
                        false),
                    new("Future Event Example", "This event will happen soon.", DateTime.UtcNow.AddMonths(1),
                        EventType.NetworkingEvent, organizerUser.Id, GetVenue("Madison").Id, false)
                };


                await context.Events.AddRangeAsync(events);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task EnsureRoleAsync(RoleManager<IdentityRole<Guid>> roleManager, string roleName)
    {
        if (!await roleManager.RoleExistsAsync(roleName)) await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
    }

    private static async Task<User> EnsureUserAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, string email, string password,
        string fName, string lName, string role, string phoneNumber)
    {
        var appUser = await userManager.FindByEmailAsync(email);
        User? domainUser = null;

        if (appUser == null)
        {
            appUser = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true, PhoneNumber = phoneNumber };
            var result = await userManager.CreateAsync(appUser, password);
             if (!result.Succeeded)
                throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            
            domainUser = new User(appUser.Id.ToString(), fName, lName, Enum.Parse<UserRole>(role), email, email, phoneNumber, "+1");
            await context.Users.AddAsync(domainUser);
            await context.SaveChangesAsync();
        }
        else
        {
            domainUser = await context.Users.FirstOrDefaultAsync(u => u.IdentityId == appUser.Id.ToString());
            if (domainUser == null)
            {
                domainUser = new User(appUser.Id.ToString(), fName, lName, Enum.Parse<UserRole>(role), email, email, phoneNumber, "+1");
                await context.Users.AddAsync(domainUser);
                await context.SaveChangesAsync();
            }
        }

        if (!await userManager.IsInRoleAsync(appUser, role)) await userManager.AddToRoleAsync(appUser, role);

        return domainUser;
    }
}