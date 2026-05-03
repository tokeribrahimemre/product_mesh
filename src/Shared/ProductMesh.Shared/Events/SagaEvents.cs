namespace ProductMesh.Shared.Events;

// SAGA Pattern events — used for choreography-based distributed transaction management.
// Each service publishes events; compensating actions are triggered on failure.

/// <summary>
/// Published by Log service after successfully logging the product creation event.
/// Completes the SAGA successfully.
/// </summary>
public record ProductLoggedEvent(Guid ProductId, Guid SagaId, DateTime LoggedAt);

/// <summary>
/// Published when a step in the product creation SAGA fails.
/// Triggers compensating action (e.g., rollback product creation).
/// </summary>
public record ProductCreationFailedEvent(Guid ProductId, Guid SagaId, string Reason, DateTime FailedAt);
