using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using ProductMesh.Product.Domain.Events;
using ProductMesh.Shared.Events;

namespace ProductMesh.Product.Infrastructure.EventHandlers;

// Domain event handlers that publish integration events to RabbitMQ via MassTransit.
// This decouples the domain model from messaging infrastructure.
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductCreatedEventHandler> _logger;

    public ProductCreatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<ProductCreatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing ProductCreatedIntegrationEvent for product {ProductId}", notification.ProductId);

        await _publishEndpoint.Publish(new ProductCreatedIntegrationEvent(
            notification.ProductId,
            notification.Name,
            notification.Price,
            notification.CreatedBy,
            DateTime.UtcNow), cancellationToken);
    }
}

public class ProductUpdatedEventHandler : INotificationHandler<ProductUpdatedDomainEvent>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProductUpdatedEventHandler> _logger;

    public ProductUpdatedEventHandler(IPublishEndpoint publishEndpoint, ILogger<ProductUpdatedEventHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Handle(ProductUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Publishing ProductUpdatedIntegrationEvent for product {ProductId}", notification.ProductId);

        await _publishEndpoint.Publish(new ProductUpdatedIntegrationEvent(
            notification.ProductId,
            notification.Name,
            notification.Price,
            notification.UpdatedBy,
            DateTime.UtcNow), cancellationToken);
    }
}
