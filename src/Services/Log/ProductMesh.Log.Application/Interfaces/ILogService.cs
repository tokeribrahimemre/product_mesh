using ProductMesh.Log.Application.DTOs;
using ProductMesh.Log.Domain.Entities;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Log.Application.Interfaces;

public interface ILogService
{
    Task<Result<IReadOnlyList<LogEntryDto>>> GetLogsAsync(LogFilterRequest filter);
    Task<Result<LogEntryDto>> GetLogByIdAsync(Guid id);
    Task CreateLogAsync(LogEntry entry);
}
