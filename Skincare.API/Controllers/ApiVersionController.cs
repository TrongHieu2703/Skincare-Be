using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/versions")]
    public class ApiVersionController : ControllerBase
    {
        private readonly ILogger<ApiVersionController> _logger;

        public ApiVersionController(ILogger<ApiVersionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetApiVersions()
        {
            try
            {
                var versions = new
                {
                    currentVersion = "2.0",
                    supportedVersions = new[]
                    {
                        new
                        {
                            version = "1.0",
                            status = "deprecated",
                            deprecationDate = "2024-12-31",
                            sunsetDate = "2025-06-30",
                            migrationGuide = "/api/docs/migration-v2"
                        },
                        new
                        {
                            version = "2.0",
                            status = "current",
                            releaseDate = "2024-01-01",
                            features = new[]
                            {
                                "Enhanced product data with ingredients and benefits",
                                "Organic and cruelty-free product filtering",
                                "Improved search with advanced filters",
                                "Better pagination and metadata",
                                "Enhanced error responses"
                            }
                        }
                    },
                    meta = new
                    {
                        timestamp = DateTime.UtcNow,
                        documentation = "/swagger/index.html"
                    }
                };

                return Ok(versions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API versions");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("migration-guide")]
        public IActionResult GetMigrationGuide()
        {
            try
            {
                var migrationGuide = new
                {
                    title = "Migration Guide: v1.0 to v2.0",
                    description = "Complete guide to migrate from API v1.0 to v2.0",
                    changes = new[]
                    {
                        new
                        {
                            type = "breaking",
                            endpoint = "GET /api/products",
                            description = "Response structure changed to include metadata",
                            v1Example = new { message = "success", data = new object[] { } },
                            v2Example = new { 
                                data = new object[] { }, 
                                pagination = new { },
                                meta = new { version = "2.0", timestamp = DateTime.UtcNow }
                            }
                        },
                        new
                        {
                            type = "enhancement",
                            endpoint = "GET /api/v2/products",
                            description = "New endpoint with enhanced product data",
                            newFields = new[]
                            {
                                "ingredients",
                                "benefits", 
                                "usageInstructions",
                                "isOrganic",
                                "isCrueltyFree",
                                "size",
                                "stockQuantity",
                                "tags",
                                "images",
                                "metadata"
                            }
                        },
                        new
                        {
                            type = "new",
                            endpoint = "GET /api/v2/products/organic",
                            description = "New endpoint to filter organic products"
                        },
                        new
                        {
                            type = "new",
                            endpoint = "GET /api/v2/products/cruelty-free",
                            description = "New endpoint to filter cruelty-free products"
                        }
                    },
                    deprecationTimeline = new
                    {
                        deprecationDate = "2024-12-31",
                        sunsetDate = "2025-06-30",
                        recommendations = new[]
                        {
                            "Start migrating to v2.0 immediately",
                            "Update client applications to handle new response structure",
                            "Test new endpoints in development environment",
                            "Plan for complete migration before sunset date"
                        }
                    },
                    meta = new
                    {
                        version = "1.0",
                        lastUpdated = DateTime.UtcNow
                    }
                };

                return Ok(migrationGuide);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting migration guide");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("changelog")]
        public IActionResult GetChangelog()
        {
            try
            {
                var changelog = new
                {
                    versions = new[]
                    {
                        new
                        {
                            version = "2.0.0",
                            date = "2024-01-01",
                            changes = new
                            {
                                added = new[]
                                {
                                    "Enhanced ProductDto with ingredients, benefits, and metadata",
                                    "Organic and cruelty-free product filtering",
                                    "Improved search with advanced filters",
                                    "Better pagination with hasNextPage and hasPreviousPage",
                                    "Enhanced error responses with version information",
                                    "API versioning support with multiple version endpoints"
                                },
                                changed = new[]
                                {
                                    "Response structure now includes metadata object",
                                    "All endpoints return version information",
                                    "Improved error handling with structured responses"
                                },
                                deprecated = new[]
                                {
                                    "API v1.0 endpoints (will be removed in v3.0)"
                                }
                            }
                        },
                        new
                        {
                            version = "1.0.0",
                            date = "2023-01-01",
                            changes = new
                            {
                                added = new[]
                                {
                                    "Basic CRUD operations for products",
                                    "Product search and filtering",
                                    "Image upload functionality",
                                    "Authentication and authorization",
                                    "Rate limiting and caching"
                                }
                            }
                        }
                    },
                    meta = new
                    {
                        timestamp = DateTime.UtcNow
                    }
                };

                return Ok(changelog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting changelog");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult GetApiHealth()
        {
            try
            {
                var health = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    version = "2.0",
                    uptime = TimeSpan.FromDays(1), // Mock uptime
                    services = new
                    {
                        database = "connected",
                        redis = "connected",
                        authentication = "active"
                    },
                    meta = new
                    {
                        environment = "development",
                        buildNumber = "2.0.0.123"
                    }
                };

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API health");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
} 