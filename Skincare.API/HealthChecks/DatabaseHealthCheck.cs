using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Skincare.Repositories.Context;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Skincare.API.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly SWP391Context _context;
        private readonly ILogger<DatabaseHealthCheck> _logger;
        private readonly int _timeoutSeconds;

        public DatabaseHealthCheck(SWP391Context context, ILogger<DatabaseHealthCheck> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _timeoutSeconds = configuration.GetValue<int>("HealthChecks:DatabaseTimeoutSeconds", 5);
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting database health check");

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

                // Test database connection with a simple query
                var canConnect = await _context.Database.CanConnectAsync(cts.Token);
                if (!canConnect)
                {
                    _logger.LogWarning("Database health check failed: Cannot connect to database");
                    return HealthCheckResult.Unhealthy("Cannot connect to database");
                }

                // Test a simple query
                var startTime = DateTime.UtcNow;
                var accountCount = await _context.Accounts.CountAsync(cts.Token);
                var responseTime = DateTime.UtcNow - startTime;

                _logger.LogDebug("Database health check completed successfully. Response time: {ResponseTime}ms, Account count: {AccountCount}", 
                    responseTime.TotalMilliseconds, accountCount);

                var data = new Dictionary<string, object>
                {
                    { "ResponseTimeMs", responseTime.TotalMilliseconds },
                    { "AccountCount", accountCount },
                    { "DatabaseName", _context.Database.GetDbConnection().Database },
                    { "Server", _context.Database.GetDbConnection().DataSource }
                };

                return HealthCheckResult.Healthy("Database is healthy", data);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Database health check timed out after {TimeoutSeconds} seconds", _timeoutSeconds);
                return HealthCheckResult.Unhealthy("Database health check timed out");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check failed with exception");
                return HealthCheckResult.Unhealthy("Database health check failed", ex);
            }
        }
    }
} 