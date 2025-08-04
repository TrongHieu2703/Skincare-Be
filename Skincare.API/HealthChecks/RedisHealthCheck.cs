using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace Skincare.API.HealthChecks
{
    public class RedisHealthCheck : IHealthCheck
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<RedisHealthCheck> _logger;
        private readonly int _timeoutSeconds;

        public RedisHealthCheck(IConnectionMultiplexer redis, ILogger<RedisHealthCheck> logger, IConfiguration configuration)
        {
            _redis = redis;
            _logger = logger;
            _timeoutSeconds = configuration.GetValue<int>("HealthChecks:RedisTimeoutSeconds", 3);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting Redis health check");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

                var database = _redis.GetDatabase();
                var startTime = DateTime.UtcNow;

                // Test Redis connection with a simple PING command
                var pingResult = await database.PingAsync();
                var responseTime = DateTime.UtcNow - startTime;

                if (pingResult == TimeSpan.Zero)
                {
                    _logger.LogWarning("Redis health check failed: PING command failed");
                    return HealthCheckResult.Unhealthy("Redis PING command failed");
                }

                // Test a simple SET/GET operation
                var testKey = $"health_check_{Guid.NewGuid()}";
                var testValue = DateTime.UtcNow.ToString("O");
                
                await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrievedValue = await database.StringGetAsync(testKey);
                await database.KeyDeleteAsync(testKey);

                if (retrievedValue != testValue)
                {
                    _logger.LogWarning("Redis health check failed: SET/GET operation failed");
                    return HealthCheckResult.Unhealthy("Redis SET/GET operation failed");
                }

                var server = _redis.GetServer(_redis.GetEndPoints().First());
                var info = server.Info();

                _logger.LogDebug("Redis health check completed successfully. Response time: {ResponseTime}ms", 
                    responseTime.TotalMilliseconds);

                var data = new Dictionary<string, object>
                {
                    { "ResponseTimeMs", responseTime.TotalMilliseconds },
                    { "PingTimeMs", pingResult.TotalMilliseconds },
                    { "Server", server.EndPoint.ToString() },
                    { "DatabaseCount", server.DatabaseCount },
                    { "ConnectedClients", info.FirstOrDefault(x => x.Key == "connected_clients").Value },
                    { "UsedMemory", info.FirstOrDefault(x => x.Key == "used_memory_human").Value }
                };

                return HealthCheckResult.Healthy("Redis is healthy", data);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Redis health check timed out after {TimeoutSeconds} seconds", _timeoutSeconds);
                return HealthCheckResult.Unhealthy("Redis health check timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Redis health check failed with exception");
                return HealthCheckResult.Unhealthy("Redis health check failed", ex);
            }
        }
    }
} 