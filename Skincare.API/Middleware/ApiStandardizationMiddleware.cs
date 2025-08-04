using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Skincare.API.Middleware
{
    public class ApiStandardizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiStandardizationMiddleware> _logger;

        public ApiStandardizationMiddleware(RequestDelegate next, ILogger<ApiStandardizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();

            // Add correlation ID to response headers
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            context.Response.Headers["X-Request-ID"] = requestId;

            // Log request
            await LogRequestAsync(context, requestId, correlationId);

            var originalBodyStream = context.Response.Body;
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            try
            {
                await _next(context);
                stopwatch.Stop();

                // Standardize successful response
                await StandardizeResponseAsync(context, memoryStream, originalBodyStream, stopwatch.ElapsedMilliseconds, requestId, correlationId);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                await HandleExceptionAsync(context, ex, stopwatch.ElapsedMilliseconds, requestId, correlationId);
            }
            finally
            {
                // Log response
                await LogResponseAsync(context, requestId, correlationId, stopwatch.ElapsedMilliseconds);
            }
        }

        private async Task LogRequestAsync(HttpContext context, string requestId, string correlationId)
        {
            try
            {
                var logEntry = new ApiLogEntry
                {
                    RequestId = requestId,
                    CorrelationId = correlationId,
                    UserId = context.User?.Identity?.Name,
                    Endpoint = context.Request.Path,
                    Method = context.Request.Method,
                    RequestTime = DateTime.UtcNow,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString(),
                    Metadata = new Dictionary<string, object>
                    {
                        ["QueryString"] = context.Request.QueryString.ToString(),
                        ["Headers"] = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())
                    }
                };

                _logger.LogInformation("API Request: {@LogEntry}", logEntry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging request");
            }
        }

        private async Task StandardizeResponseAsync(HttpContext context, MemoryStream memoryStream, Stream originalBodyStream, long durationMs, string requestId, string correlationId)
        {
            try
            {
                memoryStream.Position = 0;
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();

                // Skip standardization for non-JSON responses or specific endpoints
                if (!IsJsonResponse(context) || ShouldSkipStandardization(context))
                {
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(originalBodyStream);
                    return;
                }

                var standardizedResponse = CreateStandardizedResponse(context, responseBody, durationMs, requestId, correlationId);
                var jsonResponse = JsonSerializer.Serialize(standardizedResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                var responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                context.Response.ContentLength = responseBytes.Length;
                await originalBodyStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error standardizing response");
                // Fallback to original response
                memoryStream.Position = 0;
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, long durationMs, string requestId, string correlationId)
        {
            try
            {
                _logger.LogError(exception, "Unhandled exception in API");

                var errorResponse = StandardApiResponse<object>.ErrorResult(
                    "An unexpected error occurred",
                    new List<ApiError>
                    {
                        ApiError.SystemError(exception.Message, exception.StackTrace)
                    }
                );

                errorResponse.Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    CorrelationId = correlationId,
                    ResponseTimeMs = durationMs,
                    RequestId = requestId,
                    Endpoint = context.Request.Path,
                    Method = context.Request.Method
                };

                var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                var responseBytes = Encoding.UTF8.GetBytes(jsonResponse);
                await context.Response.Body.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling exception");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Internal Server Error");
            }
        }

        private async Task LogResponseAsync(HttpContext context, string requestId, string correlationId, long durationMs)
        {
            try
            {
                var logEntry = new ApiLogEntry
                {
                    RequestId = requestId,
                    CorrelationId = correlationId,
                    UserId = context.User?.Identity?.Name,
                    Endpoint = context.Request.Path,
                    Method = context.Request.Method,
                    ResponseTime = DateTime.UtcNow,
                    DurationMs = durationMs,
                    StatusCode = context.Response.StatusCode,
                    IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = context.Request.Headers["User-Agent"].ToString()
                };

                if (context.Response.StatusCode >= 400)
                {
                    _logger.LogWarning("API Response Error: {@LogEntry}", logEntry);
                }
                else
                {
                    _logger.LogInformation("API Response: {@LogEntry}", logEntry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging response");
            }
        }

        private bool IsJsonResponse(HttpContext context)
        {
            return context.Response.ContentType?.Contains("application/json") == true ||
                   context.Request.Path.StartsWithSegments("/api");
        }

        private bool ShouldSkipStandardization(HttpContext context)
        {
            // Skip standardization for specific endpoints
            var skipEndpoints = new[]
            {
                "/health",
                "/swagger",
                "/favicon.ico",
                "/robots.txt"
            };

            return skipEndpoints.Any(endpoint => context.Request.Path.StartsWithSegments(endpoint));
        }

        private object CreateStandardizedResponse(HttpContext context, string responseBody, long durationMs, string requestId, string correlationId)
        {
            try
            {
                // Try to parse existing response
                if (!string.IsNullOrEmpty(responseBody))
                {
                    var existingResponse = JsonSerializer.Deserialize<object>(responseBody);
                    
                    // If it's already a standardized response, return as is
                    if (IsStandardizedResponse(existingResponse))
                    {
                        return existingResponse;
                    }

                    // Create new standardized response
                    return StandardApiResponse<object>.SuccessResult(existingResponse, "Operation completed successfully");
                }

                return StandardApiResponse<object>.SuccessResult(null, "Operation completed successfully");
            }
            catch
            {
                // If parsing fails, return as plain text response
                return StandardApiResponse<object>.SuccessResult(responseBody, "Operation completed successfully");
            }
            finally
            {
                // Update metadata
                if (context.Response.Headers.ContainsKey("X-Response-Time"))
                {
                    context.Response.Headers["X-Response-Time"] = $"{durationMs}ms";
                }
            }
        }

        private bool IsStandardizedResponse(object response)
        {
            if (response is JsonElement element)
            {
                return element.TryGetProperty("success", out _) &&
                       element.TryGetProperty("message", out _) &&
                       element.TryGetProperty("metadata", out _);
            }
            return false;
        }
    }

    public static class ApiStandardizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiStandardization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiStandardizationMiddleware>();
        }
    }
} 