using EventPlanning.Application;
using EventPlanning.Infrastructure;
using EventPlanning.Web.Extensions;

using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddPresentation(builder.Environment);
builder.Services.AddRateLimiting();

try
{
    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            await EventPlanning.Infrastructure.Persistence.DbInitializer.SeedAsync(scope.ServiceProvider, app.Environment.IsDevelopment());
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An error occurred while seeding the database.");
        }
    }

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseSecurityHeaders();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        app.UseHttpsRedirection();
    }
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();
    
    app.UseRouting();
    app.UseRateLimiter();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("Application Starting Up");
    app.Run();
}
catch (HostAbortedException)
{
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }