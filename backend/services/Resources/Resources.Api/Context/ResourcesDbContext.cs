using Microsoft.EntityFrameworkCore;
using Resources.Api.Models;

namespace Resources.Api.Context;

public class ResourcesDbContext : DbContext
{
    public ResourcesDbContext(DbContextOptions<ResourcesDbContext> options) : base(options)
    {
    }

    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<ResourceReservation> ResourceReservations => Set<ResourceReservation>();
    public DbSet<PartUsage> PartUsages => Set<PartUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(250);
            entity.Property(e => e.SerialNumber).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Quantity).HasPrecision(10, 2);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<ResourceReservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();

            entity.HasOne(e => e.Resource).WithMany(resource => resource.Reservations)
                .HasForeignKey(reserv => reserv.ResourceId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PartUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QuantityUsed).HasPrecision(10, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);

            entity.HasOne(p => p.Resource).WithMany(res => res.PartsUsages).HasForeignKey(p => p.ResourceId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}