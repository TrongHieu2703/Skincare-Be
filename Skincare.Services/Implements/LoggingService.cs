using Microsoft.Extensions.Logging;
using Skincare.Services.Interfaces;
using System.Text.Json;

namespace Skincare.Services.Implements
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogRequestStart(string endpoint, string method, string correlationId, object requestData = null)
        {
            _logger.LogInformation(
                "Request started. Endpoint: {Endpoint}, Method: {Method}, CorrelationId: {CorrelationId}, RequestData: {RequestData}",
                endpoint,
                method,
                correlationId,
                requestData != null ? JsonSerializer.Serialize(requestData) : "null");
        }

        public void LogRequestEnd(string endpoint, string method, string correlationId, int statusCode, double responseTimeMs, object responseData = null)
        {
            var logLevel = statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
            
            _logger.Log(logLevel,
                "Request completed. Endpoint: {Endpoint}, Method: {Method}, CorrelationId: {CorrelationId}, StatusCode: {StatusCode}, ResponseTime: {ResponseTime}ms, ResponseData: {ResponseData}",
                endpoint,
                method,
                correlationId,
                statusCode,
                responseTimeMs,
                responseData != null ? JsonSerializer.Serialize(responseData) : "null");
        }

        public void LogBusinessOperation(string operation, string correlationId, object data = null)
        {
            _logger.LogInformation(
                "Business operation executed. Operation: {Operation}, CorrelationId: {CorrelationId}, Data: {Data}",
                operation,
                correlationId,
                data != null ? JsonSerializer.Serialize(data) : "null");
        }

        public void LogCacheOperation(string operation, string key, string correlationId, bool success, double durationMs = 0)
        {
            var logLevel = success ? LogLevel.Debug : LogLevel.Warning;
            
            _logger.Log(logLevel,
                "Cache operation. Operation: {Operation}, Key: {Key}, CorrelationId: {CorrelationId}, Success: {Success}, Duration: {Duration}ms",
                operation,
                key,
                correlationId,
                success,
                durationMs);
        }

        public void LogDatabaseOperation(string operation, string correlationId, double durationMs, bool success, string details = null)
        {
            var logLevel = success ? LogLevel.Debug : LogLevel.Error;
            
            _logger.Log(logLevel,
                "Database operation. Operation: {Operation}, CorrelationId: {CorrelationId}, Duration: {Duration}ms, Success: {Success}, Details: {Details}",
                operation,
                correlationId,
                durationMs,
                success,
                details ?? "null");
        }

        public void LogSecurityEvent(string eventType, string correlationId, string userId = null, string details = null)
        {
            _logger.LogWarning(
                "Security event detected. EventType: {EventType}, CorrelationId: {CorrelationId}, UserId: {UserId}, Details: {Details}",
                eventType,
                correlationId,
                userId ?? "anonymous",
                details ?? "null");
        }

        public void LogPerformanceMetric(string metricName, double value, string correlationId, string unit = null)
        {
            _logger.LogInformation(
                "Performance metric. Metric: {MetricName}, Value: {Value}{Unit}, CorrelationId: {CorrelationId}",
                metricName,
                value,
                unit ?? "",
                correlationId);
        }

        public void LogUserActivity(string activity, string correlationId, string userId, object data = null)
        {
            _logger.LogInformation(
                "User activity. Activity: {Activity}, CorrelationId: {CorrelationId}, UserId: {UserId}, Data: {Data}",
                activity,
                correlationId,
                userId,
                data != null ? JsonSerializer.Serialize(data) : "null");
        }
    }
} 