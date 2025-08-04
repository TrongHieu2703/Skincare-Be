using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        // Sales Analytics Endpoints
        [HttpGet("sales")]
        public async Task<IActionResult> GetSalesAnalytics([FromQuery] DateTime? date = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                SalesAnalyticsDto analytics;

                if (date.HasValue)
                {
                    analytics = await _analyticsService.GetSalesAnalyticsAsync(date.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    analytics = await _analyticsService.GetSalesAnalyticsAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    analytics = await _analyticsService.GetSalesAnalyticsAsync(DateTime.Today);
                }

                var response = ApiResponse<SalesAnalyticsDto>.SuccessResult(analytics, "Sales analytics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales analytics");
                var errorResponse = ApiResponse<SalesAnalyticsDto>.ErrorResult(
                    "Failed to get sales analytics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("sales/category")]
        public async Task<IActionResult> GetSalesByCategory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var salesByCategory = await _analyticsService.GetSalesByCategoryAsync(startDate, endDate);

                var response = ApiResponse<List<SalesByCategoryDto>>.SuccessResult(salesByCategory, "Sales by category retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by category");
                var errorResponse = ApiResponse<List<SalesByCategoryDto>>.ErrorResult(
                    "Failed to get sales by category",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("sales/top-products")]
        public async Task<IActionResult> GetTopSellingProducts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int limit = 10)
        {
            try
            {
                var topProducts = await _analyticsService.GetTopSellingProductsAsync(startDate, endDate, limit);

                var response = ApiResponse<List<SalesByProductDto>>.SuccessResult(topProducts, "Top selling products retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top selling products");
                var errorResponse = ApiResponse<List<SalesByProductDto>>.ErrorResult(
                    "Failed to get top selling products",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("sales/time")]
        public async Task<IActionResult> GetSalesByTime([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string timeGroup = "day")
        {
            try
            {
                var salesByTime = await _analyticsService.GetSalesByTimeAsync(startDate, endDate, timeGroup);

                var response = ApiResponse<List<SalesByTimeDto>>.SuccessResult(salesByTime, "Sales by time retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by time");
                var errorResponse = ApiResponse<List<SalesByTimeDto>>.ErrorResult(
                    "Failed to get sales by time",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("sales/region")]
        public async Task<IActionResult> GetSalesByRegion([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var salesByRegion = await _analyticsService.GetSalesByRegionAsync(startDate, endDate);

                var response = ApiResponse<List<SalesByRegionDto>>.SuccessResult(salesByRegion, "Sales by region retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sales by region");
                var errorResponse = ApiResponse<List<SalesByRegionDto>>.ErrorResult(
                    "Failed to get sales by region",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // User Behavior Analytics Endpoints
        [HttpGet("user-behavior")]
        public async Task<IActionResult> GetUserBehaviorAnalytics([FromQuery] DateTime? date = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                UserBehaviorAnalyticsDto analytics;

                if (date.HasValue)
                {
                    analytics = await _analyticsService.GetUserBehaviorAnalyticsAsync(date.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    analytics = await _analyticsService.GetUserBehaviorAnalyticsAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    analytics = await _analyticsService.GetUserBehaviorAnalyticsAsync(DateTime.Today);
                }

                var response = ApiResponse<UserBehaviorAnalyticsDto>.SuccessResult(analytics, "User behavior analytics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user behavior analytics");
                var errorResponse = ApiResponse<UserBehaviorAnalyticsDto>.ErrorResult(
                    "Failed to get user behavior analytics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("user-behavior/activities")]
        public async Task<IActionResult> GetUserActivities([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string userId = null)
        {
            try
            {
                var activities = await _analyticsService.GetUserActivitiesAsync(startDate, endDate, userId);

                var response = ApiResponse<List<UserActivityDto>>.SuccessResult(activities, "User activities retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user activities");
                var errorResponse = ApiResponse<List<UserActivityDto>>.ErrorResult(
                    "Failed to get user activities",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("user-behavior/journeys")]
        public async Task<IActionResult> GetUserJourneys([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string journeyType = null)
        {
            try
            {
                var journeys = await _analyticsService.GetUserJourneysAsync(startDate, endDate, journeyType);

                var response = ApiResponse<List<UserJourneyDto>>.SuccessResult(journeys, "User journeys retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user journeys");
                var errorResponse = ApiResponse<List<UserJourneyDto>>.ErrorResult(
                    "Failed to get user journeys",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("user-behavior/segments")]
        public async Task<IActionResult> GetUserSegments([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var segments = await _analyticsService.GetUserSegmentsAsync(startDate, endDate);

                var response = ApiResponse<List<UserSegmentDto>>.SuccessResult(segments, "User segments retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user segments");
                var errorResponse = ApiResponse<List<UserSegmentDto>>.ErrorResult(
                    "Failed to get user segments",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Performance Metrics Endpoints
        [HttpGet("performance")]
        public async Task<IActionResult> GetPerformanceMetrics([FromQuery] DateTime? date = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                PerformanceMetricsDto metrics;

                if (date.HasValue)
                {
                    metrics = await _analyticsService.GetPerformanceMetricsAsync(date.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    metrics = await _analyticsService.GetPerformanceMetricsAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    metrics = await _analyticsService.GetPerformanceMetricsAsync(DateTime.Today);
                }

                var response = ApiResponse<PerformanceMetricsDto>.SuccessResult(metrics, "Performance metrics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance metrics");
                var errorResponse = ApiResponse<PerformanceMetricsDto>.ErrorResult(
                    "Failed to get performance metrics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("performance/endpoints")]
        public async Task<IActionResult> GetPerformanceByEndpoint([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var performanceByEndpoint = await _analyticsService.GetPerformanceByEndpointAsync(startDate, endDate);

                var response = ApiResponse<List<PerformanceByEndpointDto>>.SuccessResult(performanceByEndpoint, "Performance by endpoint retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting performance by endpoint");
                var errorResponse = ApiResponse<List<PerformanceByEndpointDto>>.ErrorResult(
                    "Failed to get performance by endpoint",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("performance/system-resources")]
        public async Task<IActionResult> GetSystemResourceUsage()
        {
            try
            {
                var systemResources = await _analyticsService.GetSystemResourceUsageAsync();

                var response = ApiResponse<SystemResourceUsageDto>.SuccessResult(systemResources, "System resource usage retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting system resource usage");
                var errorResponse = ApiResponse<SystemResourceUsageDto>.ErrorResult(
                    "Failed to get system resource usage",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Inventory Analytics Endpoints
        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryAnalytics([FromQuery] DateTime? date = null, [FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            try
            {
                InventoryAnalyticsDto analytics;

                if (date.HasValue)
                {
                    analytics = await _analyticsService.GetInventoryAnalyticsAsync(date.Value);
                }
                else if (startDate.HasValue && endDate.HasValue)
                {
                    analytics = await _analyticsService.GetInventoryAnalyticsAsync(startDate.Value, endDate.Value);
                }
                else
                {
                    analytics = await _analyticsService.GetInventoryAnalyticsAsync(DateTime.Today);
                }

                var response = ApiResponse<InventoryAnalyticsDto>.SuccessResult(analytics, "Inventory analytics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory analytics");
                var errorResponse = ApiResponse<InventoryAnalyticsDto>.ErrorResult(
                    "Failed to get inventory analytics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("inventory/low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10)
        {
            try
            {
                var lowStockProducts = await _analyticsService.GetLowStockProductsAsync(threshold);

                var response = ApiResponse<List<LowStockProductDto>>.SuccessResult(lowStockProducts, "Low stock products retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting low stock products");
                var errorResponse = ApiResponse<List<LowStockProductDto>>.ErrorResult(
                    "Failed to get low stock products",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("inventory/movements")]
        public async Task<IActionResult> GetStockMovements([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var stockMovements = await _analyticsService.GetStockMovementsAsync(startDate, endDate);

                var response = ApiResponse<List<StockMovementDto>>.SuccessResult(stockMovements, "Stock movements retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stock movements");
                var errorResponse = ApiResponse<List<StockMovementDto>>.ErrorResult(
                    "Failed to get stock movements",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Reporting Endpoints
        [HttpPost("reports/generate")]
        public async Task<IActionResult> GenerateReport([FromBody] ReportRequestDto request)
        {
            try
            {
                var report = await _analyticsService.GenerateReportAsync(request);

                var response = ApiResponse<ReportResponseDto>.SuccessResult(report, "Report generated successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report");
                var errorResponse = ApiResponse<ReportResponseDto>.ErrorResult(
                    "Failed to generate report",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("reports/{reportId}")]
        public async Task<IActionResult> GetReport(string reportId)
        {
            try
            {
                var report = await _analyticsService.GetReportAsync(reportId);

                if (report == null)
                {
                    var errorResponse = ApiResponse<ReportResponseDto>.ErrorResult(
                        "Report not found",
                        new List<ApiError> { ApiError.BusinessError("REPORT_NOT_FOUND", "Report with specified ID does not exist") }
                    );
                    return NotFound(errorResponse);
                }

                var response = ApiResponse<ReportResponseDto>.SuccessResult(report, "Report retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report");
                var errorResponse = ApiResponse<ReportResponseDto>.ErrorResult(
                    "Failed to get report",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("reports/history")]
        public async Task<IActionResult> GetReportHistory([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var reportHistory = await _analyticsService.GetReportHistoryAsync(startDate, endDate);

                var response = ApiResponse<List<ReportResponseDto>>.SuccessResult(reportHistory, "Report history retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report history");
                var errorResponse = ApiResponse<List<ReportResponseDto>>.ErrorResult(
                    "Failed to get report history",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Dashboard Endpoints
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var dashboardData = await _analyticsService.GetDashboardDataAsync(startDate, endDate);

                var response = ApiResponse<Dictionary<string, object>>.SuccessResult(dashboardData, "Dashboard data retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard data");
                var errorResponse = ApiResponse<Dictionary<string, object>>.ErrorResult(
                    "Failed to get dashboard data",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("dashboard/charts")]
        public async Task<IActionResult> GetDashboardCharts([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var charts = await _analyticsService.GetDashboardChartsAsync(startDate, endDate);

                var response = ApiResponse<List<ChartDataDto>>.SuccessResult(charts, "Dashboard charts retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard charts");
                var errorResponse = ApiResponse<List<ChartDataDto>>.ErrorResult(
                    "Failed to get dashboard charts",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("dashboard/key-metrics")]
        public async Task<IActionResult> GetKeyMetrics([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var keyMetrics = await _analyticsService.GetKeyMetricsAsync(startDate, endDate);

                var response = ApiResponse<Dictionary<string, object>>.SuccessResult(keyMetrics, "Key metrics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting key metrics");
                var errorResponse = ApiResponse<Dictionary<string, object>>.ErrorResult(
                    "Failed to get key metrics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("dashboard/insights")]
        public async Task<IActionResult> GetKeyInsights([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var insights = await _analyticsService.GetKeyInsightsAsync(startDate, endDate);

                var response = ApiResponse<List<string>>.SuccessResult(insights, "Key insights retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting key insights");
                var errorResponse = ApiResponse<List<string>>.ErrorResult(
                    "Failed to get key insights",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("dashboard/recommendations")]
        public async Task<IActionResult> GetRecommendations([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var recommendations = await _analyticsService.GetRecommendationsAsync(startDate, endDate);

                var response = ApiResponse<List<string>>.SuccessResult(recommendations, "Recommendations retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommendations");
                var errorResponse = ApiResponse<List<string>>.ErrorResult(
                    "Failed to get recommendations",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Real-time Analytics Endpoints
        [HttpGet("realtime/metrics")]
        public async Task<IActionResult> GetRealTimeMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetRealTimeMetricsAsync();

                var response = ApiResponse<Dictionary<string, object>>.SuccessResult(metrics, "Real-time metrics retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting real-time metrics");
                var errorResponse = ApiResponse<Dictionary<string, object>>.ErrorResult(
                    "Failed to get real-time metrics",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("realtime/events")]
        public async Task<IActionResult> GetRealTimeEvents([FromQuery] int limit = 50)
        {
            try
            {
                var events = await _analyticsService.GetRealTimeEventsAsync(limit);

                var response = ApiResponse<List<object>>.SuccessResult(events, "Real-time events retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting real-time events");
                var errorResponse = ApiResponse<List<object>>.ErrorResult(
                    "Failed to get real-time events",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("realtime/dashboard")]
        public async Task<IActionResult> GetLiveDashboard()
        {
            try
            {
                var dashboard = await _analyticsService.GetLiveDashboardAsync();

                var response = ApiResponse<Dictionary<string, object>>.SuccessResult(dashboard, "Live dashboard retrieved successfully");
                response.Metadata.CorrelationId = HttpContext.Response.Headers["X-Correlation-ID"].ToString();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting live dashboard");
                var errorResponse = ApiResponse<Dictionary<string, object>>.ErrorResult(
                    "Failed to get live dashboard",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        // Data Export Endpoints
        [HttpGet("export/sales")]
        public async Task<IActionResult> ExportSalesData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "excel")
        {
            try
            {
                var data = await _analyticsService.ExportSalesDataAsync(startDate, endDate, format);

                var fileName = $"sales_data_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    "json" => "application/json",
                    _ => "application/octet-stream"
                };

                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting sales data");
                var errorResponse = ApiResponse<object>.ErrorResult(
                    "Failed to export sales data",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("export/user-behavior")]
        public async Task<IActionResult> ExportUserBehaviorData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "excel")
        {
            try
            {
                var data = await _analyticsService.ExportUserBehaviorDataAsync(startDate, endDate, format);

                var fileName = $"user_behavior_data_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    "json" => "application/json",
                    _ => "application/octet-stream"
                };

                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting user behavior data");
                var errorResponse = ApiResponse<object>.ErrorResult(
                    "Failed to export user behavior data",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("export/performance")]
        public async Task<IActionResult> ExportPerformanceData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "excel")
        {
            try
            {
                var data = await _analyticsService.ExportPerformanceDataAsync(startDate, endDate, format);

                var fileName = $"performance_data_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    "json" => "application/json",
                    _ => "application/octet-stream"
                };

                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting performance data");
                var errorResponse = ApiResponse<object>.ErrorResult(
                    "Failed to export performance data",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }

        [HttpGet("export/inventory")]
        public async Task<IActionResult> ExportInventoryData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string format = "excel")
        {
            try
            {
                var data = await _analyticsService.ExportInventoryDataAsync(startDate, endDate, format);

                var fileName = $"inventory_data_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
                var contentType = format.ToLower() switch
                {
                    "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "csv" => "text/csv",
                    "json" => "application/json",
                    _ => "application/octet-stream"
                };

                return File(data, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting inventory data");
                var errorResponse = ApiResponse<object>.ErrorResult(
                    "Failed to export inventory data",
                    new List<ApiError> { ApiError.SystemError(ex.Message) }
                );
                return StatusCode(500, errorResponse);
            }
        }
    }
} 