using EventPlanning.Application;
using EventPlanning.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("newsletter-limit", limiterOptions =>
    {
        limiterOptions.PermitLimit = 3;
        limiterOptions.Window = TimeSpan.FromHours(1);
        limiterOptions.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
});

var app = builder.Build();

// SECURE DB SEEDING
using (var scope = app.Services.CreateScope())
{
    try
    {
        await EventPlanning.Infrastructure.Persistence.DbInitializer.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database. Ensure User Secrets are configured.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();