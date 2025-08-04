using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public ApiMetadata Metadata { get; set; }
        public List<ApiError> Errors { get; set; } = new List<ApiError>();

        public static ApiResponse<T> SuccessResult(T data, string message = "Operation completed successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0"
                }
            };
        }

        public static ApiResponse<T> ErrorResult(string message, List<ApiError> errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default(T),
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0"
                },
                Errors = errors ?? new List<ApiError>()
            };
        }

        public static ApiResponse<T> PaginatedResult(T data, int totalCount, int pageNumber, int pageSize, string message = "Data retrieved successfully")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                Metadata = new ApiMetadata
                {
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0",
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            };
        }
    }

    public class ApiMetadata
    {
        public DateTime Timestamp { get; set; }
        public string Version { get; set; }
        public int? TotalCount { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TotalPages { get; set; }
        public string CorrelationId { get; set; }
        public double? ResponseTimeMs { get; set; }
    }

    public class ApiError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
        public string Field { get; set; }
        public string HelpUrl { get; set; }
        public string Severity { get; set; } = "error";

        public static ApiError ValidationError(string field, string message, string details = null)
        {
            return new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = message,
                Details = details,
                Field = field,
                Severity = "warning"
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
                Severity = "critical"
            };
        }
    }
} 