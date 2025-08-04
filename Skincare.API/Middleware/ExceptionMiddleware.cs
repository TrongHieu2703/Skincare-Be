using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Exceptions;
using System.Net;
using System.Text.Json;

namespace Skincare.API.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var (statusCode, message, details) = exception switch
            {
                NotFoundException => (HttpStatusCode.NotFound, exception.Message, "Resource not found"),
                BusinessLogicException => (HttpStatusCode.BadRequest, exception.Message, "Business logic error"),
                DuplicateEmailException => (HttpStatusCode.Conflict, exception.Message, "Email already exists"),
                DuplicatePhoneNumberException => (HttpStatusCode.Conflict, exception.Message, "Phone number already exists"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Access denied", "Unauthorized access"),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message, "Invalid argument"),
                InvalidOperationException => (HttpStatusCode.BadRequest, exception.Message, "Invalid operation"),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred", "Internal server error")
            };

            // Log the exception with structured data
            _logger.LogError(exception, 
                "Exception occurred during request processing. " +
                "CorrelationId: {CorrelationId}, " +
                "RequestPath: {RequestPath}, " +
                "RequestMethod: {RequestMethod}, " +
                "StatusCode: {StatusCode}, " +
                "UserAgent: {UserAgent}, " +
                "RemoteIP: {RemoteIP}, " +
                "ExceptionType: {ExceptionType}, " +
                "Message: {Message}",
                correlationId,
                requestPath,
                requestMethod,
                (int)statusCode,
                userAgent,
                remoteIp,
                exception.GetType().Name,
                exception.Message);

            // Log additional context for debugging
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(exception, 
                    "Stack trace for internal server error. " +
                    "CorrelationId: {CorrelationId}, " +
                    "Exception: {Exception}",
                    correlationId,
                    exception.ToString());
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                message = message,
                details = details,
                correlationId = correlationId,
                timestamp = DateTime.UtcNow,
                path = requestPath,
                method = requestMethod,
                statusCode = (int)statusCode
            };

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionMiddleware>();
        }
    }
}
