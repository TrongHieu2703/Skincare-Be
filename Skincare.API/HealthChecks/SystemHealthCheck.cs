using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Skincare.API.HealthChecks
{
    public class SystemHealthCheck : IHealthCheck
    {
        private readonly ILogger<SystemHealthCheck> _logger;
        private readonly int _memoryThresholdMB;
        private readonly int _diskThresholdPercent;

        public SystemHealthCheck(ILogger<SystemHealthCheck> logger, IConfiguration configuration)
        {
            _logger = logger;
            _memoryThresholdMB = configuration.GetValue<int>("HealthChecks:MemoryThresholdMB", 512);
            _diskThresholdPercent = configuration.GetValue<int>("HealthChecks:DiskThresholdPercent", 90);
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogDebug("Starting system health check");

                var data = new Dictionary<string, object>();
                var issues = new List<string>();

                // Check memory usage
                var memoryInfo = GetMemoryInfo();
                data.Add("MemoryUsageMB", memoryInfo.UsedMB);
                data.Add("MemoryTotalMB", memoryInfo.TotalMB);
                data.Add("MemoryUsagePercent", memoryInfo.UsagePercent);

                if (memoryInfo.UsedMB > _memoryThresholdMB)
                {
                    issues.Add($"Memory usage ({memoryInfo.UsedMB}MB) exceeds threshold ({_memoryThresholdMB}MB)");
                }

                // Check disk usage
                var diskInfo = GetDiskInfo();
                data.Add("DiskUsageGB", diskInfo.UsedGB);
                data.Add("DiskTotalGB", diskInfo.TotalGB);
                data.Add("DiskUsagePercent", diskInfo.UsagePercent);

                if (diskInfo.UsagePercent > _diskThresholdPercent)
                {
                    issues.Add($"Disk usage ({diskInfo.UsagePercent}%) exceeds threshold ({_diskThresholdPercent}%)");
                }

                // Check process information
                var processInfo = GetProcessInfo();
                data.Add("ProcessId", processInfo.ProcessId);
                data.Add("ProcessUptimeMinutes", processInfo.UptimeMinutes);
                data.Add("ProcessMemoryMB", processInfo.MemoryMB);
                data.Add("ProcessCpuTime", processInfo.CpuTime);

                // Check application directory
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                data.Add("ApplicationDirectory", appDirectory);
                data.Add("ApplicationDirectoryExists", Directory.Exists(appDirectory));

                if (!Directory.Exists(appDirectory))
                {
                    issues.Add("Application directory does not exist");
                }

                _logger.LogDebug("System health check completed. Memory: {MemoryUsage}MB/{MemoryTotal}MB, Disk: {DiskUsage}%", 
                    memoryInfo.UsedMB, memoryInfo.TotalMB, diskInfo.UsagePercent);

                if (issues.Count > 0)
                {
                    return Task.FromResult(HealthCheckResult.Degraded(
                        $"System health check found {issues.Count} issue(s): {string.Join(", ", issues)}", 
                        data: data));
                }

                return Task.FromResult(HealthCheckResult.Healthy("System is healthy", data));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "System health check failed with exception");
                return Task.FromResult(HealthCheckResult.Unhealthy("System health check failed", ex));
            }
        }

        private (long UsedMB, long TotalMB, double UsagePercent) GetMemoryInfo()
        {
            var process = Process.GetCurrentProcess();
            var usedMB = process.WorkingSet64 / (1024 * 1024);
            
            // Get total system memory
            var totalMB = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / (1024 * 1024);
            var usagePercent = totalMB > 0 ? (double)usedMB / totalMB * 100 : 0;

            return (usedMB, totalMB, Math.Round(usagePercent, 2));
        }

        private (long UsedGB, long TotalGB, double UsagePercent) GetDiskInfo()
        {
            var drive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory));
            var usedGB = (drive.TotalSize - drive.AvailableFreeSpace) / (1024 * 1024 * 1024);
            var totalGB = drive.TotalSize / (1024 * 1024 * 1024);
            var usagePercent = totalGB > 0 ? (double)usedGB / totalGB * 100 : 0;

            return (usedGB, totalGB, Math.Round(usagePercent, 2));
        }

        private (int ProcessId, double UptimeMinutes, long MemoryMB, TimeSpan CpuTime) GetProcessInfo()
        {
            var process = Process.GetCurrentProcess();
            var uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
            var memoryMB = process.WorkingSet64 / (1024 * 1024);

            return (process.Id, uptime.TotalMinutes, memoryMB, process.TotalProcessorTime);
        }
    }
} 