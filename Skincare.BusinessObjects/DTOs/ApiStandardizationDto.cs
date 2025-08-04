using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Skincare.BusinessObjects.DTOs
{
    // Standardized API Response Structure
    public class StandardApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ApiMetadata Metadata { get; set; }
        public List<ApiError> Errors { get; set; } = new List<ApiError>();
        public ApiPagination Pagination { get; set; }
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();

        // Factory methods for creating responses
        public static StandardApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new StandardApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    CorrelationId = Guid.NewGuid().ToString(),
                    ResponseTimeMs = 0
                }
            };
        }

        public static StandardApiResponse<T> ErrorResult(string message, List<ApiError> errors = null)
        {
            return new StandardApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Errors = errors ?? new List<ApiError>(),
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    CorrelationId = Guid.NewGuid().ToString(),
                    ResponseTimeMs = 0
                }
            };
        }

        public static StandardApiResponse<T> PaginatedResult(T data, int totalItems, int pageNumber, int pageSize, string message = "Data retrieved successfully")
        {
            return new StandardApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Pagination = new ApiPagination
                {
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                    HasPreviousPage = pageNumber > 1,
                    HasNextPage = pageNumber < (int)Math.Ceiling((double)totalItems / pageSize)
                },
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    CorrelationId = Guid.NewGuid().ToString(),
                    ResponseTimeMs = 0
                }
            };
        }
    }

    // API Metadata
    public class ApiMetadata
    {
        public DateTime Timestamp { get; set; }
        public string Version { get; set; }
        public string CorrelationId { get; set; }
        public double ResponseTimeMs { get; set; }
        public string RequestId { get; set; }
        public string UserId { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; } = new Dictionary<string, object>();
    }

    // API Error Structure
    public class ApiError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Field { get; set; }
        public string HelpUrl { get; set; }
        public string Severity { get; set; } = "error"; // error, warning, info
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

        // Predefined error types
        public static ApiError ValidationError(string field, string message, string details = null)
        {
            return new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = message,
                Details = details,
                Field = field,
                Severity = "error"
            };
        }

        public static ApiError BusinessError(string code, string message, string details = null)
        {
            return new ApiError
            {
                Code = code,
                Message = message,
                Details = details,
                Severity = "error"
            };
        }

        public static ApiError SystemError(string message, string details = null)
        {
            return new ApiError
            {
                Code = "SYSTEM_ERROR",
                Message = message,
                Details = details,
                Severity = "error"
            };
        }

        public static ApiError NotFoundError(string resource, string identifier = null)
        {
            return new ApiError
            {
                Code = "NOT_FOUND",
                Message = $"{resource} not found",
                Details = identifier != null ? $"Resource with identifier '{identifier}' was not found" : null,
                Severity = "error"
            };
        }

        public static ApiError UnauthorizedError(string message = "Unauthorized access")
        {
            return new ApiError
            {
                Code = "UNAUTHORIZED",
                Message = message,
                Severity = "error"
            };
        }

        public static ApiError ForbiddenError(string message = "Access forbidden")
        {
            return new ApiError
            {
                Code = "FORBIDDEN",
                Message = message,
                Severity = "error"
            };
        }
    }

    // API Pagination
    public class ApiPagination
    {
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }
        public string PreviousPageUrl { get; set; }
        public string NextPageUrl { get; set; }
        public string FirstPageUrl { get; set; }
        public string LastPageUrl { get; set; }
    }

    // API Request Base
    public abstract class ApiRequest
    {
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        public DateTime RequestTimestamp { get; set; } = DateTime.UtcNow;
        public string ClientVersion { get; set; }
        public string ClientId { get; set; }
        public Dictionary<string, object> Extensions { get; set; } = new Dictionary<string, object>();
    }

    // API Response Base
    public abstract class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public ApiMetadata Metadata { get; set; }
        public List<ApiError> Errors { get; set; } = new List<ApiError>();
    }

    // API Documentation
    public class ApiEndpointInfo
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public List<ApiParameter> Parameters { get; set; } = new List<ApiParameter>();
        public List<ApiResponseExample> ResponseExamples { get; set; } = new List<ApiResponseExample>();
        public bool RequiresAuthentication { get; set; }
        public List<string> RequiredRoles { get; set; } = new List<string>();
        public string Version { get; set; }
        public bool IsDeprecated { get; set; }
        public string DeprecationMessage { get; set; }
        public DateTime? DeprecationDate { get; set; }
    }

    public class ApiParameter
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
        public string Example { get; set; }
        public List<string> AllowedValues { get; set; } = new List<string>();
        public string ValidationRules { get; set; }
    }

    public class ApiResponseExample
    {
        public int StatusCode { get; set; }
        public string Description { get; set; }
        public object Example { get; set; }
        public string ContentType { get; set; } = "application/json";
    }

    // API Versioning
    public class ApiVersionInfo
    {
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Status { get; set; } // stable, beta, deprecated
        public List<string> Changes { get; set; } = new List<string>();
        public List<string> BreakingChanges { get; set; } = new List<string>();
        public string MigrationGuide { get; set; }
        public DateTime? DeprecationDate { get; set; }
        public string DeprecationMessage { get; set; }
    }

    // API Health Check
    public class ApiHealthStatus
    {
        public string Status { get; set; } // healthy, degraded, unhealthy
        public DateTime CheckedAt { get; set; }
        public double ResponseTime { get; set; }
        public Dictionary<string, object> Components { get; set; } = new Dictionary<string, object>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Errors { get; set; } = new List<string>();
    }

    // API Rate Limiting
    public class ApiRateLimitInfo
    {
        public int Limit { get; set; }
        public int Remaining { get; set; }
        public DateTime ResetTime { get; set; }
        public string Policy { get; set; }
        public Dictionary<string, int> LimitsByEndpoint { get; set; } = new Dictionary<string, int>();
    }

    // API Caching
    public class ApiCacheInfo
    {
        public bool Cached { get; set; }
        public DateTime CachedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string CacheKey { get; set; }
        public string CachePolicy { get; set; }
        public TimeSpan TimeToLive { get; set; }
    }

    // API Performance Metrics
    public class ApiPerformanceMetrics
    {
        public double ResponseTimeMs { get; set; }
        public double DatabaseQueryTimeMs { get; set; }
        public double CacheHitRate { get; set; }
        public int MemoryUsageMB { get; set; }
        public double CpuUsage { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<string, double> EndpointPerformance { get; set; } = new Dictionary<string, double>();
    }

    // API Security
    public class ApiSecurityInfo
    {
        public bool Authenticated { get; set; }
        public string UserId { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
        public string TokenType { get; set; }
        public DateTime TokenExpiresAt { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public Dictionary<string, object> SecurityContext { get; set; } = new Dictionary<string, object>();
    }

    // API Request/Response Logging
    public class ApiLogEntry
    {
        public string RequestId { get; set; }
        public string CorrelationId { get; set; }
        public string UserId { get; set; }
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public double DurationMs { get; set; }
        public int StatusCode { get; set; }
        public string RequestBody { get; set; }
        public string ResponseBody { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public List<ApiError> Errors { get; set; } = new List<ApiError>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
} 