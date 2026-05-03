namespace ProductMesh.Product.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category);

public record UpdateProductRequest(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category);
