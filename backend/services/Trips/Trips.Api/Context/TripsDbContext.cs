using Microsoft.EntityFrameworkCore;
using Trips.Api.Models;

namespace Trips.Api.Context;

public class TripsDbContext : DbContext
{
    public TripsDbContext(DbContextOptions<TripsDbContext> options) : base(options)
    {
    }

    public DbSet<Trip> Trips => Set<Trip>();
    public DbSet<TripDestination> TripDestinations => Set<TripDestination>();
    public DbSet<TripAssignment> TripAssignments => Set<TripAssignment>();
    public DbSet<TripNumberHelper> TripNumberHelpers => Set<TripNumberHelper>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Trip>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TripNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Description).HasMaxLength(250);
            entity.HasIndex(e => e.TripNumber).IsUnique();
        });

        modelBuilder.Entity<TripDestination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LocationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
            
            entity.Property(e => e.Longitude).HasPrecision(9, 6);
            entity.Property(e => e.Latitude).HasPrecision(9, 6);
            
            entity.HasOne(e => e.Trip).WithMany(trip => trip.Destinations).HasForeignKey(dest => dest.TripId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TripId, e.WorkerId }).IsUnique();
            entity.HasOne(e => e.Trip).WithMany(t => t.Assignments).HasForeignKey(assignment => assignment.TripId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TripNumberHelper>(entity =>
        {
            entity.HasKey(e => e.Date);
        });
    }
}