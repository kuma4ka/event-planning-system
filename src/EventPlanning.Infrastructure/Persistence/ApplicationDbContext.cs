using EventPlanning.Domain.Entities;
using EventPlanning.Domain.ValueObjects;
using EventPlanning.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public new DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<NewsletterSubscriber> NewsletterSubscribers => Set<NewsletterSubscriber>();
    public DbSet<Guest> Guests => Set<Guest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(u => u.Id);
            
            entity.Property(u => u.UserName).IsRequired();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.FirstName).IsRequired();
            entity.Property(u => u.LastName).IsRequired();
            
            entity.Property(u => u.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null);
        });

        builder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.HasOne(e => e.Venue)
                  .WithMany(v => v.Events)
                  .HasForeignKey(e => e.VenueId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(e => e.OrganizerId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        builder.Entity<Venue>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(v => v.OrganizerId)
                  .IsRequired()
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(v => !v.IsDeleted);
        });

        builder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.Id);

            entity.Property(g => g.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(
                    v => v.Value,
                    v => EmailAddress.Create(v));

            entity.Property(g => g.PhoneNumber)
                .HasConversion(
                    v => v != null ? v.Value : null,
                    v => v != null ? PhoneNumber.Create(v) : null);

            entity.HasIndex(g => new { g.EventId, g.Email })
                .IsUnique();

            entity.HasOne(g => g.Event)
                .WithMany(e => e.Guests)
                .HasForeignKey(g => g.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}