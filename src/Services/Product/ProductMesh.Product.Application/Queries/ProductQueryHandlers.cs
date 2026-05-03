using MediatR;
using ProductMesh.Product.Application.DTOs;
using ProductMesh.Product.Application.Interfaces;
using ProductMesh.Product.Domain.Interfaces;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Product.Application.Queries;

// Cache-aside pattern: check Redis cache first → on miss → query database → populate cache.
// This optimizes read-heavy product listing operations.
public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;

    public GetProductByIdQueryHandler(IProductRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"product:{request.Id}";
        var cached = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<ProductDto>.Success(cached);

        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result<ProductDto>.Failure("Product not found.");

        var dto = MapToDto(product);
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(10), cancellationToken);
        return Result<ProductDto>.Success(dto);
    }

    private static ProductDto MapToDto(Domain.Entities.Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.CreatedAt, p.UpdatedAt);
}

public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, Result<IReadOnlyList<ProductDto>>>
{
    private readonly IProductRepository _repository;
    private readonly ICacheService _cacheService;

    public GetAllProductsQueryHandler(IProductRepository repository, ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<Result<IReadOnlyList<ProductDto>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:page:{request.Page}:size:{request.PageSize}";
        var cached = await _cacheService.GetAsync<IReadOnlyList<ProductDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<IReadOnlyList<ProductDto>>.Success(cached);

        var products = await _repository.GetAllAsync(cancellationToken);
        var paged = products
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToDto)
            .ToList();

        await _cacheService.SetAsync(cacheKey, (IReadOnlyList<ProductDto>)paged, TimeSpan.FromMinutes(5), cancellationToken);
        return Result<IReadOnlyList<ProductDto>>.Success(paged);
    }

    private static ProductDto MapToDto(Domain.Entities.Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.CreatedAt, p.UpdatedAt);
}
