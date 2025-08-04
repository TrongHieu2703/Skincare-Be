using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Skincare.API.Middleware
{
    public static class RateLimitingMiddleware
    {
        public static IServiceCollection AddCustomRateLimiting(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRateLimiter(options =>
            {
                // Global rate limiter
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = int.Parse(configuration["RateLimiting:PermitLimit"]),
                            Window = TimeSpan.Parse(configuration["RateLimiting:Window"]),
                            SegmentsPerWindow = int.Parse(configuration["RateLimiting:SegmentsPerWindow"]),
                            QueueLimit = int.Parse(configuration["RateLimiting:QueueLimit"])
                        }));

                // Specific rate limiter for authentication endpoints
                options.AddPolicy("AuthPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 5, // 5 requests per window for auth endpoints
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 1,
                            QueueLimit = 0
                        }));

                // Rate limiter for file uploads
                options.AddPolicy("UploadPolicy", context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10, // 10 uploads per window
                            Window = TimeSpan.FromMinutes(5),
                            SegmentsPerWindow = 1,
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = 429;
                    context.HttpContext.Response.ContentType = "application/json";
                    
                    var response = new
                    {
                        message = "Too many requests. Please try again later.",
                        retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                            ? retryAfter.TotalSeconds 
                            : 60
                    };
                    
                    await context.HttpContext.Response.WriteAsJsonAsync(response, token);
                };
            });

            return services;
        }
    }
} 