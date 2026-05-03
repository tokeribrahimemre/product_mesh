using MediatR;

namespace ProductMesh.Product.Domain.Events;

// Domain events following Domain-Driven Design — raised within the domain layer,
// handled by Application layer to trigger side effects (cache invalidation, integration events).
public record ProductCreatedDomainEvent(Guid ProductId, string Name, decimal Price, string CreatedBy) : INotification;
public record ProductUpdatedDomainEvent(Guid ProductId, string Name, decimal Price, string UpdatedBy) : INotification;
