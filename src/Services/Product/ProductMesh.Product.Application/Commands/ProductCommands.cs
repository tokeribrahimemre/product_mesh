using MediatR;
using ProductMesh.Product.Application.DTOs;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Product.Application.Commands;

// CQRS Pattern: Commands represent write operations. Each command has a dedicated handler
// following Single Responsibility Principle — one handler, one responsibility.
public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    string CreatedBy) : IRequest<Result<ProductDto>>;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Category,
    string UpdatedBy) : IRequest<Result<ProductDto>>;
