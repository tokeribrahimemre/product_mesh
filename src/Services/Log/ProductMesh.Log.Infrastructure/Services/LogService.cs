using ProductMesh.Log.Application.DTOs;
using ProductMesh.Log.Application.Interfaces;
using ProductMesh.Log.Domain.Entities;
using ProductMesh.Log.Domain.Interfaces;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Log.Infrastructure.Services;

public class LogService : ILogService
{
    private readonly ILogRepository _repository;

    public LogService(ILogRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IReadOnlyList<LogEntryDto>>> GetLogsAsync(LogFilterRequest filter)
    {
        var entries = await _repository.GetByFilterAsync(
            filter.ServiceName, filter.Level, filter.From, filter.To,
            filter.Page, filter.PageSize);

        var dtos = entries.Select(MapToDto).ToList() as IReadOnlyList<LogEntryDto>;
        return Result<IReadOnlyList<LogEntryDto>>.Success(dtos);
    }

    public async Task<Result<LogEntryDto>> GetLogByIdAsync(Guid id)
    {
        var entry = await _repository.GetByIdAsync(id);
        if (entry is null) return Result<LogEntryDto>.Failure("Log entry not found.");
        return Result<LogEntryDto>.Success(MapToDto(entry));
    }

    public async Task CreateLogAsync(LogEntry entry)
    {
        await _repository.AddAsync(entry);
    }

    private static LogEntryDto MapToDto(LogEntry e) =>
        new(e.Id, e.ServiceName, e.Level, e.Message, e.Exception, e.CorrelationId, e.Timestamp);
}
