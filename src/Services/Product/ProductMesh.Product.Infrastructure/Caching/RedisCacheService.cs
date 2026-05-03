using System.Text.Json;
using ProductMesh.Product.Application.Interfaces;
using StackExchange.Redis;

namespace ProductMesh.Product.Infrastructure.Caching;

// Redis Cache Service implementing the cache-aside pattern.
// Read: check cache → miss → DB → populate cache
// Write: update DB → invalidate cache (done in command handlers)
public class RedisCacheService : ICacheService
{
    private readonly IDatabase _database;
    private readonly IConnectionMultiplexer _connection;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public RedisCacheService(IConnectionMultiplexer connection)
    {
        _connection = connection;
        _database = connection.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value, JsonOptions);
        await _database.StringSetAsync(key, json, expiration ?? TimeSpan.FromMinutes(10));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var endpoints = _connection.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = _connection.GetServer(endpoint);
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Length > 0)
                await _database.KeyDeleteAsync(keys);
        }
    }
}
