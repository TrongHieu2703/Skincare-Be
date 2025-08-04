using Microsoft.Extensions.Logging;

namespace Skincare.Services.Interfaces
{
    public interface ILoggingService
    {
        void LogRequestStart(string endpoint, string method, string correlationId, object requestData = null);
        void LogRequestEnd(string endpoint, string method, string correlationId, int statusCode, double responseTimeMs, object responseData = null);
        void LogBusinessOperation(string operation, string correlationId, object data = null);
        void LogCacheOperation(string operation, string key, string correlationId, bool success, double durationMs = 0);
        void LogDatabaseOperation(string operation, string correlationId, double durationMs, bool success, string details = null);
        void LogSecurityEvent(string eventType, string correlationId, string userId = null, string details = null);
        void LogPerformanceMetric(string metricName, double value, string correlationId, string unit = null);
        void LogUserActivity(string activity, string correlationId, string userId, object data = null);
    }
} 