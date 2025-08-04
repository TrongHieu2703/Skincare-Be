using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync();
                
                var response = new
                {
                    status = healthReport.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    totalDuration = healthReport.TotalDuration.TotalMilliseconds,
                    entries = healthReport.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        duration = entry.Value.Duration.TotalMilliseconds,
                        data = entry.Value.Data,
                        tags = entry.Value.Tags
                    }),
                    meta = new
                    {
                        version = "1.0",
                        environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                    }
                };

                var statusCode = healthReport.Status switch
                {
                    HealthStatus.Healthy => 200,
                    HealthStatus.Degraded => 200, // Still operational but with issues
                    HealthStatus.Unhealthy => 503, // Service unavailable
                    _ => 500
                };

                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting health status");
                return StatusCode(500, new { 
                    status = "Error", 
                    message = "Health check failed", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("ready")]
        public async Task<IActionResult> GetReadiness()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync(registration => 
                    registration.Tags.Contains("ready"));

                var isReady = healthReport.Status == HealthStatus.Healthy;
                var statusCode = isReady ? 200 : 503;

                var response = new
                {
                    ready = isReady,
                    status = healthReport.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    checks = healthReport.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description
                    })
                };

                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting readiness status");
                return StatusCode(503, new { 
                    ready = false, 
                    message = "Readiness check failed", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("live")]
        public async Task<IActionResult> GetLiveness()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync(registration => 
                    registration.Tags.Contains("live"));

                var isAlive = healthReport.Status != HealthStatus.Unhealthy;
                var statusCode = isAlive ? 200 : 503;

                var response = new
                {
                    alive = isAlive,
                    status = healthReport.Status.ToString(),
                    timestamp = DateTime.UtcNow,
                    uptime = Environment.TickCount64 / 1000.0, // Uptime in seconds
                    checks = healthReport.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description
                    })
                };

                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting liveness status");
                return StatusCode(503, new { 
                    alive = false, 
                    message = "Liveness check failed", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealth()
        {
            try
            {
                var healthReport = await _healthCheckService.CheckHealthAsync();
                
                var detailedResponse = new
                {
                    overall = new
                    {
                        status = healthReport.Status.ToString(),
                        timestamp = DateTime.UtcNow,
                        totalDuration = healthReport.TotalDuration.TotalMilliseconds
                    },
                    services = new
                    {
                        database = GetServiceHealth(healthReport, "database"),
                        redis = GetServiceHealth(healthReport, "redis"),
                        system = GetServiceHealth(healthReport, "system"),
                        responseTime = GetServiceHealth(healthReport, "response-time")
                    },
                    summary = new
                    {
                        healthy = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                        degraded = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                        unhealthy = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
                        total = healthReport.Entries.Count
                    },
                    environment = new
                    {
                        name = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                        machineName = Environment.MachineName,
                        osVersion = Environment.OSVersion.ToString(),
                        processorCount = Environment.ProcessorCount,
                        workingSet = Environment.WorkingSet / (1024 * 1024), // MB
                        version = Environment.Version.ToString()
                    }
                };

                var statusCode = healthReport.Status switch
                {
                    HealthStatus.Healthy => 200,
                    HealthStatus.Degraded => 200,
                    HealthStatus.Unhealthy => 503,
                    _ => 500
                };

                return StatusCode(statusCode, detailedResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed health status");
                return StatusCode(500, new { 
                    status = "Error", 
                    message = "Detailed health check failed", 
                    details = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        private object GetServiceHealth(HealthReport healthReport, string serviceName)
        {
            var entry = healthReport.Entries.FirstOrDefault(e => e.Key.ToLower().Contains(serviceName));
            
            if (entry.Value == null)
            {
                return new { status = "Unknown", description = "Service not found" };
            }

            return new
            {
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                data = entry.Value.Data
            };
        }
    }
} 