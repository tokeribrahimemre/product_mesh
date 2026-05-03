using ProductMesh.Shared.Entities;

namespace ProductMesh.Log.Domain.Entities;

// Structured log entry with severity levels for centralized log management.
// Severity levels: INFO, WARNING, ERROR, CRITICAL as required.
public class LogEntry : BaseEntity<Guid>
{
    public string ServiceName { get; set; } = string.Empty;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? CorrelationId { get; set; }
    public string? Properties { get; set; } // JSON-serialized additional properties
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public enum LogLevel
{
    INFO = 0,
    WARNING = 1,
    ERROR = 2,
    CRITICAL = 3
}
