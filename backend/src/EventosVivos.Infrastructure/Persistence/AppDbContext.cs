using EventosVivos.Domain.Entities;
using EventosVivos.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EventosVivos.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>(entity =>
        {
            entity.ToTable("Venues");
            entity.HasKey(v => v.Id);
            entity.Property(v => v.Id).ValueGeneratedNever();
            entity.Property(v => v.Name).HasMaxLength(200).IsRequired();
            entity.Property(v => v.City).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("Events");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.TicketPrice).HasPrecision(18, 2);
            entity.HasOne(e => e.Venue)
                .WithMany()
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(e => e.Reservations)
                .WithOne(r => r.Event)
                .HasForeignKey(r => r.EventId);
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(r => r.Id);
            entity.Property(r => r.BuyerName).HasMaxLength(200).IsRequired();
            entity.Property(r => r.BuyerEmail).HasMaxLength(256).IsRequired();
            entity.Property(r => r.ReservationCode).HasMaxLength(20);
            entity.HasIndex(r => r.ReservationCode).IsUnique();
        });

        SeedVenues(modelBuilder);
    }

    private static void SeedVenues(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Venue>().HasData(
            new { Id = 1, Name = "Auditorio Central", Capacity = 200, City = "Bogotá" },
            new { Id = 2, Name = "Sala Norte", Capacity = 50, City = "Bogotá" },
            new { Id = 3, Name = "Arena Sur", Capacity = 500, City = "Medellín" });
    }
}
