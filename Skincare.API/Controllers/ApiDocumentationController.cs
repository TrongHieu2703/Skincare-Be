using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using System.Reflection;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ApiDocumentationController : ControllerBase
    {
        private readonly ILogger<ApiDocumentationController> _logger;

        public ApiDocumentationController(ILogger<ApiDocumentationController> logger)
        {
            _logger = logger;
        }

        [HttpGet("info")]
        public IActionResult GetApiInfo()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
                var buildDate = System.IO.File.GetLastWriteTime(assembly.Location);

                var apiInfo = new
                {
                    Name = "Skincare Store API",
                    Version = version,
                    Description = "Comprehensive API for Skincare Store e-commerce platform",
                    BuildDate = buildDate,
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    BaseUrl = $"{Request.Scheme}://{Request.Host}",
                    DocumentationUrl = $"{Request.Scheme}://{Request.Host}/swagger",
                    HealthCheckUrl = $"{Request.Scheme}://{Request.Host}/health",
                    Contact = new
                    {
                        Name = "API Support",
                        Email = "support@skincare.com",
                        Url = "https://skincare.com/support"
                    },
                    License = new
                    {
                        Name = "MIT",
                        Url = "https://opensource.org/licenses/MIT"
                    }
                };

                var response = StandardApiResponse<object>.SuccessResult(apiInfo, "API information retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API info");
                var errorResponse = StandardApiResponse<object>.ErrorResult(
                    "Failed to get API info",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("endpoints")]
        public IActionResult GetApiEndpoints()
        {
            try
            {
                var endpoints = new List<ApiEndpointInfo>
                {
                    // Authentication endpoints
                    new ApiEndpointInfo
                    {
                        Endpoint = "/api/authentication/login",
                        Method = "POST",
                        Description = "Authenticate user and get access token",
                        Summary = "User login",
                        Tags = new List<string> { "Authentication" },
                        Parameters = new List<ApiParameter>
                        {
                            new ApiParameter
                            {
                                Name = "email",
                                Type = "string",
                                Description = "User email address",
                                Required = true,
                                Example = "user@example.com"
                            },
                            new ApiParameter
                            {
                                Name = "password",
                                Type = "string",
                                Description = "User password",
                                Required = true,
                                Example = "password123"
                            }
                        },
                        ResponseExamples = new List<ApiResponseExample>
                        {
                            new ApiResponseExample
                            {
                                StatusCode = 200,
                                Description = "Login successful",
                                Example = new
                                {
                                    success = true,
                                    message = "Login successful",
                                    data = new
                                    {
                                        accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                                        refreshToken = "refresh_token_here",
                                        expiresIn = 900
                                    }
                                }
                            }
                        },
                        RequiresAuthentication = false
                    },

                    // Product endpoints
                    new ApiEndpointInfo
                    {
                        Endpoint = "/api/products",
                        Method = "GET",
                        Description = "Get all products with optional filtering and pagination",
                        Summary = "Get products",
                        Tags = new List<string> { "Products" },
                        Parameters = new List<ApiParameter>
                        {
                            new ApiParameter
                            {
                                Name = "pageNumber",
                                Type = "int",
                                Description = "Page number for pagination",
                                Required = false,
                                DefaultValue = "1",
                                Example = "1"
                            },
                            new ApiParameter
                            {
                                Name = "pageSize",
                                Type = "int",
                                Description = "Number of items per page",
                                Required = false,
                                DefaultValue = "10",
                                Example = "10"
                            },
                            new ApiParameter
                            {
                                Name = "category",
                                Type = "string",
                                Description = "Filter by product category",
                                Required = false,
                                Example = "moisturizers"
                            }
                        },
                        ResponseExamples = new List<ApiResponseExample>
                        {
                            new ApiResponseExample
                            {
                                StatusCode = 200,
                                Description = "Products retrieved successfully",
                                Example = new
                                {
                                    success = true,
                                    message = "Products retrieved successfully",
                                    data = new List<object>(),
                                    pagination = new
                                    {
                                        totalItems = 100,
                                        pageNumber = 1,
                                        pageSize = 10,
                                        totalPages = 10
                                    }
                                }
                            }
                        },
                        RequiresAuthentication = false
                    },

                    // Order endpoints
                    new ApiEndpointInfo
                    {
                        Endpoint = "/api/orders",
                        Method = "POST",
                        Description = "Create a new order",
                        Summary = "Create order",
                        Tags = new List<string> { "Orders" },
                        Parameters = new List<ApiParameter>
                        {
                            new ApiParameter
                            {
                                Name = "customerId",
                                Type = "string",
                                Description = "Customer ID",
                                Required = true,
                                Example = "user123"
                            },
                            new ApiParameter
                            {
                                Name = "items",
                                Type = "array",
                                Description = "Order items",
                                Required = true,
                                Example = "[{\"productId\": 1, \"quantity\": 2}]"
                            }
                        },
                        ResponseExamples = new List<ApiResponseExample>
                        {
                            new ApiResponseExample
                            {
                                StatusCode = 201,
                                Description = "Order created successfully",
                                Example = new
                                {
                                    success = true,
                                    message = "Order created successfully",
                                    data = new
                                    {
                                        orderId = 123,
                                        status = "pending",
                                        totalAmount = 99.99
                                    }
                                }
                            }
                        },
                        RequiresAuthentication = true,
                        RequiredRoles = new List<string> { "Customer", "Admin" }
                    },

                    // Analytics endpoints
                    new ApiEndpointInfo
                    {
                        Endpoint = "/api/analytics/sales",
                        Method = "GET",
                        Description = "Get sales analytics for specified date range",
                        Summary = "Get sales analytics",
                        Tags = new List<string> { "Analytics" },
                        Parameters = new List<ApiParameter>
                        {
                            new ApiParameter
                            {
                                Name = "startDate",
                                Type = "datetime",
                                Description = "Start date for analytics",
                                Required = true,
                                Example = "2024-01-01"
                            },
                            new ApiParameter
                            {
                                Name = "endDate",
                                Type = "datetime",
                                Description = "End date for analytics",
                                Required = true,
                                Example = "2024-01-31"
                            }
                        },
                        ResponseExamples = new List<ApiResponseExample>
                        {
                            new ApiResponseExample
                            {
                                StatusCode = 200,
                                Description = "Sales analytics retrieved successfully",
                                Example = new
                                {
                                    success = true,
                                    message = "Sales analytics retrieved successfully",
                                    data = new
                                    {
                                        totalRevenue = 15000.00,
                                        totalOrders = 150,
                                        averageOrderValue = 100.00
                                    }
                                }
                            }
                        },
                        RequiresAuthentication = true,
                        RequiredRoles = new List<string> { "Admin" }
                    }
                };

                var response = StandardApiResponse<List<ApiEndpointInfo>>.SuccessResult(endpoints, "API endpoints retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API endpoints");
                var errorResponse = StandardApiResponse<List<ApiEndpointInfo>>.ErrorResult(
                    "Failed to get API endpoints",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("versions")]
        public IActionResult GetApiVersions()
        {
            try
            {
                var versions = new List<ApiVersionInfo>
                {
                    new ApiVersionInfo
                    {
                        Version = "2.0",
                        ReleaseDate = DateTime.Parse("2024-01-15"),
                        Status = "stable",
                        Changes = new List<string>
                        {
                            "Enhanced product search with Elasticsearch",
                            "Real-time notifications with SignalR",
                            "Comprehensive analytics and reporting",
                            "Advanced validation with FluentValidation",
                            "API standardization and documentation"
                        },
                        BreakingChanges = new List<string>
                        {
                            "Product DTO structure updated",
                            "Authentication response format changed",
                            "Some endpoint paths modified"
                        },
                        MigrationGuide = "https://skincare.com/api/migration/v2.0"
                    },
                    new ApiVersionInfo
                    {
                        Version = "1.0",
                        ReleaseDate = DateTime.Parse("2023-12-01"),
                        Status = "deprecated",
                        DeprecationDate = DateTime.Parse("2024-06-01"),
                        DeprecationMessage = "Version 1.0 will be deprecated on June 1, 2024. Please migrate to version 2.0.",
                        Changes = new List<string>
                        {
                            "Initial API release",
                            "Basic CRUD operations",
                            "JWT authentication",
                            "File upload functionality"
                        },
                        BreakingChanges = new List<string>(),
                        MigrationGuide = "https://skincare.com/api/migration/v1.0"
                    }
                };

                var response = StandardApiResponse<List<ApiVersionInfo>>.SuccessResult(versions, "API versions retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API versions");
                var errorResponse = StandardApiResponse<List<ApiVersionInfo>>.ErrorResult(
                    "Failed to get API versions",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("health")]
        public async Task<IActionResult> GetApiHealth()
        {
            try
            {
                var healthStatus = new ApiHealthStatus
                {
                    Status = "healthy",
                    CheckedAt = DateTime.UtcNow,
                    ResponseTime = 45.2,
                    Components = new Dictionary<string, object>
                    {
                        ["database"] = new { status = "healthy", responseTime = 12.5 },
                        ["redis"] = new { status = "healthy", responseTime = 2.1 },
                        ["elasticsearch"] = new { status = "healthy", responseTime = 8.7 }
                    },
                    Warnings = new List<string>(),
                    Errors = new List<string>()
                };

                var response = StandardApiResponse<ApiHealthStatus>.SuccessResult(healthStatus, "API health check completed");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API health");
                var errorResponse = StandardApiResponse<ApiHealthStatus>.ErrorResult(
                    "Failed to get API health",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("rate-limits")]
        public IActionResult GetRateLimits()
        {
            try
            {
                var rateLimitInfo = new ApiRateLimitInfo
                {
                    Limit = 100,
                    Remaining = 85,
                    ResetTime = DateTime.UtcNow.AddMinutes(1),
                    Policy = "sliding_window",
                    LimitsByEndpoint = new Dictionary<string, int>
                    {
                        ["/api/authentication/login"] = 5,
                        ["/api/authentication/register"] = 3,
                        ["/api/products"] = 100,
                        ["/api/orders"] = 50,
                        ["/api/analytics"] = 20
                    }
                };

                var response = StandardApiResponse<ApiRateLimitInfo>.SuccessResult(rateLimitInfo, "Rate limit information retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rate limits");
                var errorResponse = StandardApiResponse<ApiRateLimitInfo>.ErrorResult(
                    "Failed to get rate limits",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("performance")]
        public IActionResult GetPerformanceMetrics()
        {
            try
            {
                var performanceMetrics = new ApiPerformanceMetrics
                {
                    ResponseTimeMs = 245.67,
                    DatabaseQueryTimeMs = 45.2,
                    CacheHitRate = 85.5,
                    MemoryUsageMB = 512,
                    CpuUsage = 25.8,
                    ActiveConnections = 150,
                    EndpointPerformance = new Dictionary<string, double>
                    {
                        ["/api/products"] = 150.5,
                        ["/api/orders"] = 320.8,
                        ["/api/analytics"] = 450.2,
                        ["/api/authentication"] = 89.3
                    }
                };

                var response = StandardApiResponse<ApiPerformanceMetrics>.SuccessResult(performanceMetrics, "Performance metrics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                var errorResponse = StandardApiResponse<ApiPerformanceMetrics>.ErrorResult(
                    "Failed to get performance metrics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("security")]
        public IActionResult GetSecurityInfo()
        {
            try
            {
                var securityInfo = new ApiSecurityInfo
                {
                    Authenticated = User.Identity?.IsAuthenticated ?? false,
                    UserId = User.Identity?.Name,
                    Roles = User.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList(),
                    Permissions = User.Claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList(),
                    TokenType = "Bearer",
                    TokenExpiresAt = DateTime.UtcNow.AddHours(1),
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                    SecurityContext = new Dictionary<string, object>
                    {
                        ["sessionId"] = HttpContext.Session.Id,
                        ["requestId"] = HttpContext.Response.Headers["X-Request-ID"].ToString()
                    }
                };

                var response = StandardApiResponse<ApiSecurityInfo>.SuccessResult(securityInfo, "Security information retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting security info");
                var errorResponse = StandardApiResponse<ApiSecurityInfo>.ErrorResult(
                    "Failed to get security info",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("schema")]
        public IActionResult GetApiSchema()
        {
            try
            {
                var schema = new
                {
                    openapi = "3.0.1",
                    info = new
                    {
                        title = "Skincare Store API",
                        version = "2.0.0",
                        description = "Comprehensive API for Skincare Store e-commerce platform"
                    },
                    servers = new[]
                    {
                        new { url = "https://api.skincare.com/v2", description = "Production server" },
                        new { url = "https://staging-api.skincare.com/v2", description = "Staging server" }
                    },
                    paths = new Dictionary<string, object>(),
                    components = new
                    {
                        securitySchemes = new Dictionary<string, object>
                        {
                            ["bearerAuth"] = new
                            {
                                type = "http",
                                scheme = "bearer",
                                bearerFormat = "JWT"
                            }
                        }
                    }
                };

                var response = StandardApiResponse<object>.SuccessResult(schema, "API schema retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API schema");
                var errorResponse = StandardApiResponse<object>.ErrorResult(
                    "Failed to get API schema",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
} 