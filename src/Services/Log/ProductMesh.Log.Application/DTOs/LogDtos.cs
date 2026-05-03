using ProductMesh.Log.Domain.Entities;

namespace ProductMesh.Log.Application.DTOs;

public record LogEntryDto(
    Guid Id,
    string ServiceName,
    LogLevel Level,
    string Message,
    string? Exception,
    string? CorrelationId,
    DateTime Timestamp);

public record LogFilterRequest(
    string? ServiceName = null,
    LogLevel? Level = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 50);
