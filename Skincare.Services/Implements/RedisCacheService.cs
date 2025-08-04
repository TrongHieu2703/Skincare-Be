using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skincare.Services.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace Skincare.Services.Implements
{
    public class RedisCacheService : ICacheService
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisCacheService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _instanceName;
        private readonly ILoggingService _loggingService;

        public RedisCacheService(
            IConnectionMultiplexer redis,
            ILogger<RedisCacheService> logger,
            IConfiguration configuration,
            ILoggingService loggingService)
        {
            _redis = redis;
            _logger = logger;
            _configuration = configuration;
            _loggingService = loggingService;
            _instanceName = configuration["Redis:InstanceName"] ?? "SkincareAPI:";
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            var correlationId = GetCorrelationId();
            var fullKey = _instanceName + key;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var database = _redis.GetDatabase();
                var value = await database.StringGetAsync(fullKey);

                stopwatch.Stop();
                var success = value.HasValue;

                _loggingService.LogCacheOperation("Get", key, correlationId, success, stopwatch.ElapsedMilliseconds);

                if (success)
                {
                    _logger.LogDebug("Cache hit for key: {Key}", key);
                    return JsonSerializer.Deserialize<T>(value!);
                }

                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogCacheOperation("Get", key, correlationId, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            var correlationId = GetCorrelationId();
            var fullKey = _instanceName + key;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var database = _redis.GetDatabase();
                var serializedValue = JsonSerializer.Serialize(value);
                await database.StringSetAsync(fullKey, serializedValue, expiration);

                stopwatch.Stop();
                _loggingService.LogCacheOperation("Set", key, correlationId, true, stopwatch.ElapsedMilliseconds);

                _logger.LogDebug("Value cached successfully for key: {Key}, Expiration: {Expiration}", key, expiration);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogCacheOperation("Set", key, correlationId, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            var correlationId = GetCorrelationId();
            var fullKey = _instanceName + key;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var database = _redis.GetDatabase();
                await database.KeyDeleteAsync(fullKey);

                stopwatch.Stop();
                _loggingService.LogCacheOperation("Remove", key, correlationId, true, stopwatch.ElapsedMilliseconds);

                _logger.LogDebug("Cache key removed: {Key}", key);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogCacheOperation("Remove", key, correlationId, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "Error removing cache key: {Key}", key);
            }
        }

        public async Task RemoveByPatternAsync(string pattern)
        {
            var correlationId = GetCorrelationId();
            var fullPattern = _instanceName + pattern;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var keys = server.Keys(pattern: fullPattern);
                var database = _redis.GetDatabase();

                var deletedCount = 0;
                foreach (var key in keys)
                {
                    await database.KeyDeleteAsync(key);
                    deletedCount++;
                }

                stopwatch.Stop();
                _loggingService.LogCacheOperation("RemoveByPattern", pattern, correlationId, true, stopwatch.ElapsedMilliseconds);

                _logger.LogInformation("Removed {Count} cache keys matching pattern: {Pattern}", deletedCount, pattern);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogCacheOperation("RemoveByPattern", pattern, correlationId, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "Error removing cache keys by pattern: {Pattern}", pattern);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            var correlationId = GetCorrelationId();
            var fullKey = _instanceName + key;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var database = _redis.GetDatabase();
                var exists = await database.KeyExistsAsync(fullKey);

                stopwatch.Stop();
                _loggingService.LogCacheOperation("Exists", key, correlationId, true, stopwatch.ElapsedMilliseconds);

                _logger.LogDebug("Cache key exists check: {Key} = {Exists}", key, exists);
                return exists;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogCacheOperation("Exists", key, correlationId, false, stopwatch.ElapsedMilliseconds);
                _logger.LogError(ex, "Error checking if cache key exists: {Key}", key);
                return false;
            }
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            var correlationId = GetCorrelationId();
            var cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                _logger.LogDebug("Cache hit for GetOrSet, key: {Key}", key);
                return cachedValue;
            }

            _logger.LogDebug("Cache miss for GetOrSet, executing factory for key: {Key}", key);
            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiration);
            }

            return value;
        }

        private string GetCorrelationId()
        {
            // In a real application, you would get this from the current request context
            // For now, we'll generate a new one or use a placeholder
            return System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString();
        }
    }
} 