using ProductMesh.Shared.Entities;

namespace ProductMesh.Shared.Events;

/// <summary>
/// Tracks the state of a distributed SAGA across microservices.
/// SAGA Pattern: choreography-based — each service reacts to events and publishes new ones.
/// On failure, compensating actions are triggered via CompensationReason.
/// </summary>
public class SagaState : BaseEntity<Guid>
{
    public string SagaType { get; set; } = string.Empty;
    public string CurrentStep { get; set; } = string.Empty;
    public SagaStatus Status { get; set; } = SagaStatus.Started;
    public string? CompensationReason { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum SagaStatus
{
    Started,
    ProductCreated,
    Logged,
    Completed,
    Failed,
    Compensated
}
