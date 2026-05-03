using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductMesh.Product.Domain.Interfaces;

namespace ProductMesh.Product.Infrastructure.Persistence;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _context;

    public ProductRepository(ProductDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Products.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Product>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Products.OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Domain.Entities.Product>> FindAsync(
        Expression<Func<Domain.Entities.Product, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.Products.Where(predicate).ToListAsync(cancellationToken);

    public async Task<Domain.Entities.Product> AddAsync(Domain.Entities.Product entity, CancellationToken cancellationToken = default)
    {
        await _context.Products.AddAsync(entity, cancellationToken);
        return entity;
    }

    public Task UpdateAsync(Domain.Entities.Product entity, CancellationToken cancellationToken = default)
    {
        _context.Products.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Domain.Entities.Product entity, CancellationToken cancellationToken = default)
    {
        _context.Products.Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Domain.Entities.Product>> GetByCategoryAsync(
        string category, CancellationToken cancellationToken = default)
        => await _context.Products.Where(p => p.Category == category)
            .OrderByDescending(p => p.CreatedAt).ToListAsync(cancellationToken);
}
