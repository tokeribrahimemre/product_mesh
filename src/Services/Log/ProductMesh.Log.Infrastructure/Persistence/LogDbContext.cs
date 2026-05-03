using Microsoft.EntityFrameworkCore;
using ProductMesh.Log.Domain.Entities;

namespace ProductMesh.Log.Infrastructure.Persistence;

public class LogDbContext : DbContext
{
    public LogDbContext(DbContextOptions<LogDbContext> options) : base(options) { }

    public DbSet<LogEntry> LogEntries => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ServiceName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Level).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.HasIndex(e => e.ServiceName);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
