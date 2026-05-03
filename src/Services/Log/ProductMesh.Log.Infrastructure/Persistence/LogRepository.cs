using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProductMesh.Log.Domain.Entities;
using ProductMesh.Log.Domain.Interfaces;

namespace ProductMesh.Log.Infrastructure.Persistence;

public class LogRepository : ILogRepository
{
    private readonly LogDbContext _context;

    public LogRepository(LogDbContext context)
    {
        _context = context;
    }

    public async Task<LogEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.LogEntries.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<LogEntry>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.LogEntries.OrderByDescending(l => l.Timestamp).Take(100).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<LogEntry>> FindAsync(
        Expression<Func<LogEntry, bool>> predicate, CancellationToken cancellationToken = default)
        => await _context.LogEntries.Where(predicate).ToListAsync(cancellationToken);

    public async Task<LogEntry> AddAsync(LogEntry entity, CancellationToken cancellationToken = default)
    {
        await _context.LogEntries.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public Task UpdateAsync(LogEntry entity, CancellationToken cancellationToken = default)
    {
        _context.LogEntries.Update(entity);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public Task DeleteAsync(LogEntry entity, CancellationToken cancellationToken = default)
    {
        _context.LogEntries.Remove(entity);
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LogEntry>> GetByFilterAsync(
        string? serviceName = null,
        LogLevel? level = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LogEntries.AsQueryable();

        if (!string.IsNullOrEmpty(serviceName))
            query = query.Where(l => l.ServiceName == serviceName);
        if (level.HasValue)
            query = query.Where(l => l.Level == level.Value);
        if (from.HasValue)
            query = query.Where(l => l.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.Timestamp <= to.Value);

        return await query
            .OrderByDescending(l => l.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
