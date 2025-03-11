using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.Exceptions; 
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

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

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var errorCode = "INTERNAL_SERVER_ERROR";
            var message = "Internal Server Error";

            if (exception is NotFoundException)
            {
                statusCode = (int)HttpStatusCode.NotFound;
                errorCode = "NOT_FOUND";
                message = exception.Message;
            }
            else if (exception is DuplicateEmailException)
            {
                statusCode = (int)HttpStatusCode.Conflict;
                errorCode = "DUPLICATE_EMAIL";
                message = exception.Message;
            }

            context.Response.StatusCode = statusCode;
            var response = new
            {
                message,
                errorCode,
                stackTrace = exception.StackTrace
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
