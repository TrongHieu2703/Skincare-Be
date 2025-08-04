using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Skincare.API.HealthChecks;
using Skincare.Services.Interfaces;
using System.Diagnostics;

namespace Skincare.API.Middleware
{
    public class ResponseTimeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseTimeMiddleware> _logger;
        private readonly ResponseTimeHealthCheck _responseTimeHealthCheck;
        private readonly ILoggingService _loggingService;

        public ResponseTimeMiddleware(
            RequestDelegate next, 
            ILogger<ResponseTimeMiddleware> logger,
            ResponseTimeHealthCheck responseTimeHealthCheck,
            ILoggingService loggingService)
        {
            _next = next;
            _logger = logger;
            _responseTimeHealthCheck = responseTimeHealthCheck;
            _loggingService = loggingService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var originalBodyStream = context.Response.Body;
            var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            var endpoint = $"{context.Request.Method} {context.Request.Path}";

            try
            {
                using var memoryStream = new MemoryStream();
                context.Response.Body = memoryStream;

                // Log request start
                _loggingService.LogRequestStart(endpoint, context.Request.Method, correlationId);

                await _next(context);

                stopwatch.Stop();
                var responseTimeMs = stopwatch.ElapsedMilliseconds;

                // Record response time for health checks
                _responseTimeHealthCheck.RecordResponseTime(endpoint, responseTimeMs);

                // Add response time header
                context.Response.Headers.Add("X-Response-Time", $"{responseTimeMs}ms");

                // Log request end with performance metrics
                _loggingService.LogRequestEnd(endpoint, context.Request.Method, correlationId, 
                    context.Response.StatusCode, responseTimeMs);

                // Log performance metric
                _loggingService.LogPerformanceMetric("ResponseTime", responseTimeMs, correlationId, "ms");

                // Log slow responses
                if (responseTimeMs > 1000) // Log responses slower than 1 second
                {
                    _logger.LogWarning("Slow response detected: {Endpoint} took {ResponseTime}ms", 
                        endpoint, responseTimeMs);
                }

                // Copy response back to original stream
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                var responseTimeMs = stopwatch.ElapsedMilliseconds;
                
                _responseTimeHealthCheck.RecordResponseTime(endpoint, responseTimeMs);

                _logger.LogError(ex, "Error processing request: {Endpoint} took {ResponseTime}ms", 
                    endpoint, responseTimeMs);

                // Log failed request
                _loggingService.LogRequestEnd(endpoint, context.Request.Method, correlationId, 
                    500, responseTimeMs, new { error = ex.Message });

                // Restore original body stream
                context.Response.Body = originalBodyStream;
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }

    public static class ResponseTimeMiddlewareExtensions
    {
        public static IApplicationBuilder UseResponseTimeTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ResponseTimeMiddleware>();
        }
    }
} 