using ProductMesh.Shared.Entities;

namespace ProductMesh.Product.Domain.Entities;

public class Product : BaseEntity<Guid>, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
