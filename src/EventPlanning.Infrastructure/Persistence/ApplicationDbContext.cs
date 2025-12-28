using EventPlanning.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EventPlanning.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<User>(options)
{
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Guest> Guests => Set<Guest>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

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
        });

        builder.Entity<Guest>(entity =>
        {
            entity.HasKey(g => g.Id);
            
            entity.Property(g => g.Email)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasIndex(g => new { g.EventId, g.Email })
                .IsUnique();

            entity.HasOne(g => g.Event)
                .WithMany(e => e.Guests)
                .HasForeignKey(g => g.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}