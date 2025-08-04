using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    public class SalesAnalyticsDto
    {
        public DateTime Date { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalProducts { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal ConversionRate { get; set; }
        public List<SalesByCategoryDto> SalesByCategory { get; set; } = new List<SalesByCategoryDto>();
        public List<SalesByProductDto> TopSellingProducts { get; set; } = new List<SalesByProductDto>();
        public List<SalesByTimeDto> SalesByTime { get; set; } = new List<SalesByTimeDto>();
        public List<SalesByRegionDto> SalesByRegion { get; set; } = new List<SalesByRegionDto>();
        public Dictionary<string, decimal> RevenueByPaymentMethod { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    }

    public class SalesByCategoryDto
    {
        public string CategoryName { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int ProductCount { get; set; }
        public decimal Percentage { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class SalesByProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public decimal Revenue { get; set; }
        public int QuantitySold { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal ProfitMargin { get; set; }
        public int OrderCount { get; set; }
        public decimal Rating { get; set; }
    }

    public class SalesByTimeDto
    {
        public DateTime TimeSlot { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public decimal AverageOrderValue { get; set; }
    }

    public class SalesByRegionDto
    {
        public string Region { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
        public int CustomerCount { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal GrowthRate { get; set; }
    }

    public class UserBehaviorAnalyticsDto
    {
        public DateTime Date { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int NewUsers { get; set; }
        public int ReturningUsers { get; set; }
        public double UserRetentionRate { get; set; }
        public double UserEngagementRate { get; set; }
        public List<UserActivityDto> UserActivities { get; set; } = new List<UserActivityDto>();
        public List<UserJourneyDto> UserJourneys { get; set; } = new List<UserJourneyDto>();
        public List<UserSegmentDto> UserSegments { get; set; } = new List<UserSegmentDto>();
        public Dictionary<string, int> PageViews { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> SessionDuration { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, int> BounceRate { get; set; } = new Dictionary<string, int>();
    }

    public class UserActivityDto
    {
        public string UserId { get; set; }
        public string ActivityType { get; set; } // login, purchase, view_product, add_to_cart
        public DateTime Timestamp { get; set; }
        public string PageUrl { get; set; }
        public string ProductId { get; set; }
        public string Category { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class UserJourneyDto
    {
        public string UserId { get; set; }
        public string JourneyType { get; set; } // purchase, browse, search
        public List<string> Steps { get; set; } = new List<string>();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double Duration { get; set; }
        public bool IsCompleted { get; set; }
        public string Outcome { get; set; }
    }

    public class UserSegmentDto
    {
        public string SegmentName { get; set; }
        public string Criteria { get; set; }
        public int UserCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageOrderValue { get; set; }
        public double ConversionRate { get; set; }
        public double RetentionRate { get; set; }
        public List<string> TopProducts { get; set; } = new List<string>();
        public List<string> TopCategories { get; set; } = new List<string>();
    }

    public class PerformanceMetricsDto
    {
        public DateTime Date { get; set; }
        public double AverageResponseTime { get; set; }
        public double AveragePageLoadTime { get; set; }
        public int TotalRequests { get; set; }
        public int SuccessfulRequests { get; set; }
        public int FailedRequests { get; set; }
        public double SuccessRate { get; set; }
        public double ErrorRate { get; set; }
        public List<PerformanceByEndpointDto> PerformanceByEndpoint { get; set; } = new List<PerformanceByEndpointDto>();
        public List<PerformanceByTimeDto> PerformanceByTime { get; set; } = new List<PerformanceByTimeDto>();
        public Dictionary<string, double> DatabaseQueryTimes { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, double> CacheHitRates { get; set; } = new Dictionary<string, double>();
        public SystemResourceUsageDto SystemResources { get; set; } = new SystemResourceUsageDto();
    }

    public class PerformanceByEndpointDto
    {
        public string Endpoint { get; set; }
        public string Method { get; set; }
        public int RequestCount { get; set; }
        public double AverageResponseTime { get; set; }
        public double MinResponseTime { get; set; }
        public double MaxResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public double Throughput { get; set; }
    }

    public class PerformanceByTimeDto
    {
        public DateTime TimeSlot { get; set; }
        public int RequestCount { get; set; }
        public double AverageResponseTime { get; set; }
        public int ErrorCount { get; set; }
        public double ErrorRate { get; set; }
        public double Throughput { get; set; }
    }

    public class SystemResourceUsageDto
    {
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double DiskUsage { get; set; }
        public double NetworkUsage { get; set; }
        public int ActiveConnections { get; set; }
        public double DatabaseConnections { get; set; }
        public double CacheUsage { get; set; }
    }

    public class InventoryAnalyticsDto
    {
        public DateTime Date { get; set; }
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int OutOfStockProducts { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public List<InventoryByCategoryDto> InventoryByCategory { get; set; } = new List<InventoryByCategoryDto>();
        public List<LowStockProductDto> LowStockProductsList { get; set; } = new List<LowStockProductDto>();
        public List<StockMovementDto> StockMovements { get; set; } = new List<StockMovementDto>();
        public Dictionary<string, int> StockAlerts { get; set; } = new Dictionary<string, int>();
    }

    public class InventoryByCategoryDto
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public int InStockCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalValue { get; set; }
        public double StockTurnoverRate { get; set; }
    }

    public class LowStockProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Category { get; set; }
        public int CurrentStock { get; set; }
        public int MinimumStock { get; set; }
        public int ReorderPoint { get; set; }
        public int DaysUntilOutOfStock { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class StockMovementDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string MovementType { get; set; } // in, out, adjustment
        public int Quantity { get; set; }
        public DateTime Timestamp { get; set; }
        public string Reason { get; set; }
        public string Reference { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class ReportRequestDto
    {
        public string ReportType { get; set; } // sales, user_behavior, performance, inventory
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Format { get; set; } = "json"; // json, csv, excel, pdf
        public List<string> Filters { get; set; } = new List<string>();
        public string GroupBy { get; set; }
        public string SortBy { get; set; }
        public string SortOrder { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 100;
        public bool IncludeCharts { get; set; } = true;
        public List<string> Metrics { get; set; } = new List<string>();
    }

    public class ReportResponseDto
    {
        public string ReportId { get; set; }
        public string ReportType { get; set; }
        public DateTime GeneratedAt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public object Data { get; set; }
        public List<ChartDataDto> Charts { get; set; } = new List<ChartDataDto>();
        public ReportSummaryDto Summary { get; set; } = new ReportSummaryDto();
        public string DownloadUrl { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ChartDataDto
    {
        public string ChartType { get; set; } // line, bar, pie, area
        public string Title { get; set; }
        public string XAxisLabel { get; set; }
        public string YAxisLabel { get; set; }
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDatasetDto> Datasets { get; set; } = new List<ChartDatasetDto>();
        public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object>();
    }

    public class ChartDatasetDto
    {
        public string Label { get; set; }
        public List<object> Data { get; set; } = new List<object>();
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public int BorderWidth { get; set; } = 1;
        public bool Fill { get; set; } = false;
    }

    public class ReportSummaryDto
    {
        public int TotalRecords { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public double AverageOrderValue { get; set; }
        public double ConversionRate { get; set; }
        public double GrowthRate { get; set; }
        public List<string> KeyInsights { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }
} 