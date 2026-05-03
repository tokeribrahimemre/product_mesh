using MediatR;
using ProductMesh.Product.Application.DTOs;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Product.Application.Queries;

// CQRS Pattern: Queries represent read operations, separated from commands.
// Query operations are optimized with Redis Cache for performance.
public record GetProductByIdQuery(Guid Id) : IRequest<Result<ProductDto>>;

public record GetAllProductsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<IReadOnlyList<ProductDto>>>;
