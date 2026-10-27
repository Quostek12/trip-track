using Microsoft.EntityFrameworkCore;

namespace Workers.Api.Context;

public class WorkersDbContext : DbContext
{
    public WorkersDbContext(DbContextOptions<WorkersDbContext> options)
        : base(options)
    {
    }

    public DbSet<Models.Worker> Workers =>
        Set<Models.Worker>();
    public DbSet<Models.WorkerSkill> WorkerSkills => Set<Models.WorkerSkill>();
    public DbSet<Models.WorkerSchedule>  WorkerSchedules =>
        Set<Models.WorkerSchedule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Models.Worker>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(250);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.HasMany(worker => worker.WorkerSkills).WithOne().HasForeignKey(s => s.WorkerId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.WorkerSkill>(entity =>
        {
            entity.HasKey(skill => skill.Id);
        });

        modelBuilder.Entity<Models.WorkerSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<string>();
            entity.Property(e => e.AbsenceType).HasConversion<string>();
        });
    }
}