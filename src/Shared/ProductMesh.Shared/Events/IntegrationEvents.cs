namespace ProductMesh.Shared.Events;

// Integration events for cross-service communication via RabbitMQ
public record ProductCreatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    string CreatedBy,
    DateTime CreatedAt);

public record ProductUpdatedIntegrationEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    string UpdatedBy,
    DateTime UpdatedAt);

public record LogIntegrationEvent(
    string ServiceName,
    string Level,
    string Message,
    string? Exception,
    string? CorrelationId,
    DateTime Timestamp,
    Dictionary<string, object>? Properties);
