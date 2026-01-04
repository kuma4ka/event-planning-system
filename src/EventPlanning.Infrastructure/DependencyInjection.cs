using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Entities;
using EventPlanning.Infrastructure.Identity;
using EventPlanning.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EventPlanning.Domain.Interfaces;
using EventPlanning.Infrastructure.Repositories;
using EventPlanning.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Mapster;

namespace EventPlanning.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
                   .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.SqlServerEventId.SavepointsDisabledBecauseOfMARS)));

        services.AddMemoryCache();
        services.AddHttpContextAccessor();

            services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";

            options.Cookie.HttpOnly = true;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Lax;

            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;
        });

        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<INewsletterRepository, NewsletterRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        

        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<ICacheService, MemoryCacheService>();
        services.AddScoped<ICountryService, CountryService>();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddMapster();

        services.Configure<Application.Models.EmailSettings>(configuration.GetSection(Application.Models.EmailSettings.SectionName));
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddTransient<IEmailSender<ApplicationUser>, IdentityEmailSender>();

        return services;
    }
}