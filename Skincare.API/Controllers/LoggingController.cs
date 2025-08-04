using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class LoggingController : ControllerBase
    {
        private readonly ILogger<LoggingController> _logger;

        public LoggingController(ILogger<LoggingController> logger)
        {
            _logger = logger;
        }

        [HttpGet("files")]
        public IActionResult GetLogFiles()
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logDirectory))
                {
                    return Ok(new { message = "No log directory found", files = new string[0] });
                }

                var logFiles = Directory.GetFiles(logDirectory, "*.log")
                    .Select(file => new
                    {
                        name = Path.GetFileName(file),
                        size = new FileInfo(file).Length,
                        lastModified = File.GetLastWriteTime(file),
                        path = file
                    })
                    .OrderByDescending(f => f.lastModified)
                    .ToList();

                return Ok(new
                {
                    message = "Log files retrieved successfully",
                    count = logFiles.Count,
                    files = logFiles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving log files");
                return StatusCode(500, new { message = "Error retrieving log files", details = ex.Message });
            }
        }

        [HttpGet("files/{fileName}")]
        public IActionResult GetLogFileContent(string fileName, [FromQuery] int lines = 100)
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Log file not found" });
                }

                var fileInfo = new FileInfo(filePath);
                var allLines = System.IO.File.ReadAllLines(filePath);
                var lastLines = allLines.TakeLast(lines).ToArray();

                return Ok(new
                {
                    message = "Log file content retrieved successfully",
                    fileName = fileName,
                    totalLines = allLines.Length,
                    requestedLines = lines,
                    actualLines = lastLines.Length,
                    fileSize = fileInfo.Length,
                    lastModified = fileInfo.LastWriteTime,
                    content = lastLines
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file: {FileName}", fileName);
                return StatusCode(500, new { message = "Error reading log file", details = ex.Message });
            }
        }

        [HttpDelete("files/{fileName}")]
        public IActionResult DeleteLogFile(string fileName)
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                var filePath = Path.Combine(logDirectory, fileName);

                if (!System.IO.File.Exists(filePath))
                {
                    return NotFound(new { message = "Log file not found" });
                }

                System.IO.File.Delete(filePath);

                _logger.LogInformation("Log file deleted by admin: {FileName}", fileName);

                return Ok(new { message = "Log file deleted successfully", fileName = fileName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting log file: {FileName}", fileName);
                return StatusCode(500, new { message = "Error deleting log file", details = ex.Message });
            }
        }

        [HttpPost("clear")]
        public IActionResult ClearAllLogs()
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logDirectory))
                {
                    return Ok(new { message = "No log directory found" });
                }

                var logFiles = Directory.GetFiles(logDirectory, "*.log");
                var deletedCount = 0;

                foreach (var file in logFiles)
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                }

                _logger.LogInformation("All log files cleared by admin. Deleted {Count} files", deletedCount);

                return Ok(new { 
                    message = "All log files cleared successfully", 
                    deletedCount = deletedCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing log files");
                return StatusCode(500, new { message = "Error clearing log files", details = ex.Message });
            }
        }

        [HttpGet("stats")]
        public IActionResult GetLogStats()
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logDirectory))
                {
                    return Ok(new { message = "No log directory found", stats = new { } });
                }

                var logFiles = Directory.GetFiles(logDirectory, "*.log");
                var totalSize = logFiles.Sum(file => new FileInfo(file).Length);
                var oldestFile = logFiles.Length > 0 ? logFiles.Min(file => File.GetCreationTime(file)) : DateTime.MinValue;
                var newestFile = logFiles.Length > 0 ? logFiles.Max(file => File.GetLastWriteTime(file)) : DateTime.MinValue;

                var stats = new
                {
                    totalFiles = logFiles.Length,
                    totalSizeBytes = totalSize,
                    totalSizeMB = Math.Round(totalSize / (1024.0 * 1024.0), 2),
                    oldestFile = oldestFile,
                    newestFile = newestFile,
                    averageFileSize = logFiles.Length > 0 ? Math.Round(totalSize / (double)logFiles.Length, 2) : 0
                };

                return Ok(new
                {
                    message = "Log statistics retrieved successfully",
                    stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving log statistics");
                return StatusCode(500, new { message = "Error retrieving log statistics", details = ex.Message });
            }
        }

        [HttpPost("test")]
        public IActionResult TestLogging()
        {
            try
            {
                var correlationId = Guid.NewGuid().ToString();
                
                _logger.LogTrace("This is a TRACE log message. CorrelationId: {CorrelationId}", correlationId);
                _logger.LogDebug("This is a DEBUG log message. CorrelationId: {CorrelationId}", correlationId);
                _logger.LogInformation("This is an INFORMATION log message. CorrelationId: {CorrelationId}", correlationId);
                _logger.LogWarning("This is a WARNING log message. CorrelationId: {CorrelationId}", correlationId);
                _logger.LogError("This is an ERROR log message. CorrelationId: {CorrelationId}", correlationId);

                return Ok(new
                {
                    message = "Test log messages generated successfully",
                    correlationId = correlationId,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test log messages");
                return StatusCode(500, new { message = "Error generating test log messages", details = ex.Message });
            }
        }
    }
} 