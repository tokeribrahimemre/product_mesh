using MediatR;
using ProductMesh.Product.Application.DTOs;
using ProductMesh.Product.Application.Interfaces;
using ProductMesh.Product.Domain.Events;
using ProductMesh.Product.Domain.Interfaces;
using ProductMesh.Shared.Interfaces;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Product.Application.Commands;

// Command Handler for creating products. Uses async write to database,
// then publishes a domain event to notify other microservices.
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public CreateProductCommandHandler(IProductRepository repository, IUnitOfWork unitOfWork, IMediator mediator)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Domain.Entities.Product
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Stock = request.Stock,
            Category = request.Category,
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event — other handlers will publish integration events to RabbitMQ
        await _mediator.Publish(new ProductCreatedDomainEvent(
            product.Id, product.Name, product.Price, request.CreatedBy), cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    private static ProductDto MapToDto(Domain.Entities.Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.CreatedAt, p.UpdatedAt);
}

// Command Handler for updating products. JWT authentication is enforced at the controller level.
// After update, publishes domain event and invalidates cache.
public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly ICacheService _cacheService;

    public UpdateProductCommandHandler(
        IProductRepository repository, IUnitOfWork unitOfWork,
        IMediator mediator, ICacheService cacheService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _cacheService = cacheService;
    }

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (product is null)
            return Result<ProductDto>.Failure("Product not found.");

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.Stock = request.Stock;
        product.Category = request.Category;
        product.UpdatedBy = request.UpdatedBy;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache Invalidation — remove stale cached data after update
        await _cacheService.RemoveAsync($"product:{product.Id}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("products:", cancellationToken);

        await _mediator.Publish(new ProductUpdatedDomainEvent(
            product.Id, product.Name, product.Price, request.UpdatedBy), cancellationToken);

        return Result<ProductDto>.Success(MapToDto(product));
    }

    private static ProductDto MapToDto(Domain.Entities.Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Category, p.CreatedAt, p.UpdatedAt);
}
