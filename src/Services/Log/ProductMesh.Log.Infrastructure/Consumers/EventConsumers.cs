using System.Text.Json;
using MassTransit;
using Microsoft.Extensions.Logging;
using ProductMesh.Log.Application.Interfaces;
using ProductMesh.Log.Domain.Entities;
using ProductMesh.Shared.Events;
using LogLevel = ProductMesh.Log.Domain.Entities.LogLevel;

namespace ProductMesh.Log.Infrastructure.Consumers;

// RabbitMQ consumer for ProductCreatedIntegrationEvent.
// Consumes events published by the Product microservice and persists structured log entries.
public class ProductCreatedConsumer : IConsumer<ProductCreatedIntegrationEvent>
{
    private readonly ILogService _logService;
    private readonly ILogger<ProductCreatedConsumer> _logger;

    public ProductCreatedConsumer(ILogService logService, ILogger<ProductCreatedConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Consuming ProductCreatedIntegrationEvent for {ProductId}", evt.ProductId);

        try
        {
            await _logService.CreateLogAsync(new LogEntry
            {
                Id = Guid.NewGuid(),
                ServiceName = "ProductService",
                Level = LogLevel.INFO,
                Message = $"Product created: {evt.Name} (ID: {evt.ProductId}) by {evt.CreatedBy}",
                Timestamp = evt.CreatedAt,
                Properties = JsonSerializer.Serialize(new { evt.ProductId, evt.Name, evt.Price, evt.CreatedBy })
            });

            // SAGA Pattern: publish completion event after successful logging
            await context.Publish(new ProductLoggedEvent(evt.ProductId, Guid.NewGuid(), DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log product creation for {ProductId}", evt.ProductId);
            // SAGA compensating action: notify product service of failure
            await context.Publish(new ProductCreationFailedEvent(
                evt.ProductId, Guid.NewGuid(), ex.Message, DateTime.UtcNow));
            throw;
        }
    }
}

public class ProductUpdatedConsumer : IConsumer<ProductUpdatedIntegrationEvent>
{
    private readonly ILogService _logService;
    private readonly ILogger<ProductUpdatedConsumer> _logger;

    public ProductUpdatedConsumer(ILogService logService, ILogger<ProductUpdatedConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Consuming ProductUpdatedIntegrationEvent for {ProductId}", evt.ProductId);

        await _logService.CreateLogAsync(new LogEntry
        {
            Id = Guid.NewGuid(),
            ServiceName = "ProductService",
            Level = LogLevel.INFO,
            Message = $"Product updated: {evt.Name} (ID: {evt.ProductId}) by {evt.UpdatedBy}",
            Timestamp = evt.UpdatedAt,
            Properties = JsonSerializer.Serialize(new { evt.ProductId, evt.Name, evt.Price, evt.UpdatedBy })
        });
    }
}

// Generic log event consumer — receives structured log events from any service
public class LogEventConsumer : IConsumer<LogIntegrationEvent>
{
    private readonly ILogService _logService;
    private readonly ILogger<LogEventConsumer> _logger;

    public LogEventConsumer(ILogService logService, ILogger<LogEventConsumer> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<LogIntegrationEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation("Consuming LogIntegrationEvent from {ServiceName}", evt.ServiceName);

        var level = evt.Level.ToUpperInvariant() switch
        {
            "INFO" or "INFORMATION" => LogLevel.INFO,
            "WARNING" or "WARN" => LogLevel.WARNING,
            "ERROR" => LogLevel.ERROR,
            "CRITICAL" or "FATAL" => LogLevel.CRITICAL,
            _ => LogLevel.INFO
        };

        await _logService.CreateLogAsync(new LogEntry
        {
            Id = Guid.NewGuid(),
            ServiceName = evt.ServiceName,
            Level = level,
            Message = evt.Message,
            Exception = evt.Exception,
            CorrelationId = evt.CorrelationId,
            Timestamp = evt.Timestamp,
            Properties = evt.Properties is not null ? JsonSerializer.Serialize(evt.Properties) : null
        });
    }
}
