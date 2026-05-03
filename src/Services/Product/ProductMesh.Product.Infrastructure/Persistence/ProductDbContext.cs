using Microsoft.EntityFrameworkCore;
using ProductMesh.Shared.Interfaces;

namespace ProductMesh.Product.Infrastructure.Persistence;

public class ProductDbContext : DbContext, IUnitOfWork
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    public DbSet<Domain.Entities.Product> Products => Set<Domain.Entities.Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Domain.Entities.Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Category);
            entity.HasIndex(e => e.Name);
        });
    }

    Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
        => base.SaveChangesAsync(cancellationToken);
}
