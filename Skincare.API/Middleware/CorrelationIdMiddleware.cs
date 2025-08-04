using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Skincare.API.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeaderName = "X-Correlation-ID";
        private const string CorrelationIdPropertyName = "CorrelationId";

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);
            
            // Add correlation ID to the response headers
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;

            // Add correlation ID to the log context
            using var scope = _logger.BeginScope(new Dictionary<string, object>
            {
                [CorrelationIdPropertyName] = correlationId,
                ["RequestPath"] = context.Request.Path,
                ["RequestMethod"] = context.Request.Method,
                ["UserAgent"] = context.Request.Headers.UserAgent.ToString(),
                ["RemoteIpAddress"] = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown"
            });

            _logger.LogDebug("Request started with correlation ID: {CorrelationId}", correlationId);

            try
            {
                await _next(context);
                
                _logger.LogDebug("Request completed with correlation ID: {CorrelationId}, Status: {StatusCode}", 
                    correlationId, context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request failed with correlation ID: {CorrelationId}", correlationId);
                throw;
            }
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check if correlation ID is provided in the request headers
            if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var correlationId))
            {
                return correlationId.ToString();
            }

            // Generate a new correlation ID if not provided
            return Activity.Current?.Id ?? Activity.Current?.RootId ?? Guid.NewGuid().ToString();
        }
    }

    public static class CorrelationIdMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
} 