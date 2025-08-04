using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Skincare.API.HealthChecks
{
    public class ResponseTimeHealthCheck : IHealthCheck
    {
        private readonly ILogger<ResponseTimeHealthCheck> _logger;
        private readonly int _responseTimeThresholdMs;
        private readonly ConcurrentQueue<ResponseTimeRecord> _recentResponses;
        private readonly int _maxRecords = 100;

        public ResponseTimeHealthCheck(ILogger<ResponseTimeHealthCheck> logger, IConfiguration configuration)
        {
            _logger = logger;
            _responseTimeThresholdMs = configuration.GetValue<int>("HealthChecks:ResponseTimeThresholdMs", 1000);
            _recentResponses = new ConcurrentQueue<ResponseTimeRecord>();
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting response time health check");

                var records = _recentResponses.ToArray();
                if (records.Length == 0)
                {
                    return Task.FromResult(HealthCheckResult.Healthy("No response time data available yet"));
                }

                var averageResponseTime = records.Average(r => r.ResponseTimeMs);
                var maxResponseTime = records.Max(r => r.ResponseTimeMs);
                var minResponseTime = records.Min(r => r.ResponseTimeMs);
                var slowResponses = records.Count(r => r.ResponseTimeMs > _responseTimeThresholdMs);
                var slowResponsePercentage = (double)slowResponses / records.Length * 100;

                var data = new Dictionary<string, object>
                {
                    { "AverageResponseTimeMs", Math.Round(averageResponseTime, 2) },
                    { "MaxResponseTimeMs", maxResponseTime },
                    { "MinResponseTimeMs", minResponseTime },
                    { "SlowResponsesCount", slowResponses },
                    { "SlowResponsePercentage", Math.Round(slowResponsePercentage, 2) },
                    { "TotalRequests", records.Length },
                    { "ThresholdMs", _responseTimeThresholdMs }
                };

                var issues = new List<string>();

                if (averageResponseTime > _responseTimeThresholdMs)
                {
                    issues.Add($"Average response time ({averageResponseTime:F2}ms) exceeds threshold ({_responseTimeThresholdMs}ms)");
                }

                if (slowResponsePercentage > 10) // More than 10% of requests are slow
                {
                    issues.Add($"High percentage of slow responses ({slowResponsePercentage:F2}%)");
                }

                _logger.LogDebug("Response time health check completed. Average: {Average}ms, Max: {Max}ms, Slow: {Slow}%", 
                    averageResponseTime, maxResponseTime, slowResponsePercentage);

                if (issues.Count > 0)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"Response time health check found {issues.Count} issue(s): {string.Join(", ", issues)}", 
                        data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("Response times are healthy", data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Response time health check failed with exception");
                return Task.FromResult(HealthCheckResult.Unhealthy("Response time health check failed", ex));
            }
        }

        public void RecordResponseTime(string endpoint, double responseTimeMs)
        {
            var record = new ResponseTimeRecord
            {
                Endpoint = endpoint,
                ResponseTimeMs = responseTimeMs,
                Timestamp = DateTime.UtcNow
            };

            _recentResponses.Enqueue(record);

            // Keep only the most recent records
            while (_recentResponses.Count > _maxRecords)
            {
                _recentResponses.TryDequeue(out _);
            }
        }

        private class ResponseTimeRecord
        {
            public string Endpoint { get; set; }
            public double ResponseTimeMs { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
} 