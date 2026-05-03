using ProductMesh.Shared.Interfaces;

namespace ProductMesh.Product.Domain.Interfaces;

public interface IProductRepository : IRepository<Entities.Product>
{
    Task<IReadOnlyList<Entities.Product>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
}
