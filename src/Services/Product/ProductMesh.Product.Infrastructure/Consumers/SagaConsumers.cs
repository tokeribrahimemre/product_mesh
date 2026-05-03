using MassTransit;
using Microsoft.Extensions.Logging;
using ProductMesh.Product.Domain.Interfaces;
using ProductMesh.Shared.Events;
using ProductMesh.Shared.Interfaces;

namespace ProductMesh.Product.Infrastructure.Consumers;

// SAGA Pattern: compensating action consumer.
// When the Log service fails to process a product creation event,
// this consumer handles the failure by marking the product accordingly.
public class ProductCreationFailedConsumer : IConsumer<ProductCreationFailedEvent>
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductCreationFailedConsumer> _logger;

    public ProductCreationFailedConsumer(
        IProductRepository repository, IUnitOfWork unitOfWork,
        ILogger<ProductCreationFailedConsumer> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreationFailedEvent> context)
    {
        var evt = context.Message;
        _logger.LogWarning("SAGA compensation: Product creation failed for {ProductId}. Reason: {Reason}",
            evt.ProductId, evt.Reason);

        var product = await _repository.GetByIdAsync(evt.ProductId);
        if (product is not null)
        {
            // Compensating action: delete the product that failed downstream processing
            await _repository.DeleteAsync(product);
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation("SAGA compensation: Product {ProductId} deleted as compensating action", evt.ProductId);
        }
    }
}

// Handles successful SAGA completion
public class ProductLoggedConsumer : IConsumer<ProductLoggedEvent>
{
    private readonly ILogger<ProductLoggedConsumer> _logger;

    public ProductLoggedConsumer(ILogger<ProductLoggedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductLoggedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("SAGA completed: Product {ProductId} successfully created and logged", evt.ProductId);
        await Task.CompletedTask;
    }
}
