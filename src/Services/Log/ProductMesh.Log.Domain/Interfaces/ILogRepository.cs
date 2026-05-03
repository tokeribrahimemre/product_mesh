using ProductMesh.Log.Domain.Entities;
using ProductMesh.Shared.Interfaces;

namespace ProductMesh.Log.Domain.Interfaces;

public interface ILogRepository : IRepository<LogEntry>
{
    Task<IReadOnlyList<LogEntry>> GetByFilterAsync(
        string? serviceName = null,
        LogLevel? level = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);
}
