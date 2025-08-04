using Skincare.BusinessObjects.DTOs;

namespace Skincare.Services.Interfaces
{
    public interface IAnalyticsService
    {
        // Sales Analytics
        Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime date);
        Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesByCategoryDto>> GetSalesByCategoryAsync(DateTime startDate, DateTime endDate);
        Task<List<SalesByProductDto>> GetTopSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 10);
        Task<List<SalesByTimeDto>> GetSalesByTimeAsync(DateTime startDate, DateTime endDate, string timeGroup = "day");
        Task<List<SalesByRegionDto>> GetSalesByRegionAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, decimal>> GetRevenueByPaymentMethodAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrdersByStatusAsync(DateTime startDate, DateTime endDate);

        // User Behavior Analytics
        Task<UserBehaviorAnalyticsDto> GetUserBehaviorAnalyticsAsync(DateTime date);
        Task<UserBehaviorAnalyticsDto> GetUserBehaviorAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<UserActivityDto>> GetUserActivitiesAsync(DateTime startDate, DateTime endDate, string userId = null);
        Task<List<UserJourneyDto>> GetUserJourneysAsync(DateTime startDate, DateTime endDate, string journeyType = null);
        Task<List<UserSegmentDto>> GetUserSegmentsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetPageViewsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetSessionDurationAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetBounceRateAsync(DateTime startDate, DateTime endDate);
        Task<double> GetUserRetentionRateAsync(DateTime startDate, DateTime endDate);
        Task<double> GetUserEngagementRateAsync(DateTime startDate, DateTime endDate);

        // Performance Metrics
        Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime date);
        Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate);
        Task<List<PerformanceByEndpointDto>> GetPerformanceByEndpointAsync(DateTime startDate, DateTime endDate);
        Task<List<PerformanceByTimeDto>> GetPerformanceByTimeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetDatabaseQueryTimesAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, double>> GetCacheHitRatesAsync(DateTime startDate, DateTime endDate);
        Task<SystemResourceUsageDto> GetSystemResourceUsageAsync();

        // Inventory Analytics
        Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync(DateTime date);
        Task<InventoryAnalyticsDto> GetInventoryAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<List<InventoryByCategoryDto>> GetInventoryByCategoryAsync();
        Task<List<LowStockProductDto>> GetLowStockProductsAsync(int threshold = 10);
        Task<List<StockMovementDto>> GetStockMovementsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetStockAlertsAsync();

        // Reporting
        Task<ReportResponseDto> GenerateReportAsync(ReportRequestDto request);
        Task<ReportResponseDto> GetReportAsync(string reportId);
        Task<List<ReportResponseDto>> GetReportHistoryAsync(DateTime startDate, DateTime endDate);
        Task<bool> ScheduleReportAsync(ReportRequestDto request, string schedule);
        Task<bool> CancelScheduledReportAsync(string reportId);
        Task<string> ExportReportAsync(string reportId, string format);

        // Dashboard
        Task<Dictionary<string, object>> GetDashboardDataAsync(DateTime startDate, DateTime endDate);
        Task<List<ChartDataDto>> GetDashboardChartsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetKeyMetricsAsync(DateTime startDate, DateTime endDate);
        Task<List<string>> GetKeyInsightsAsync(DateTime startDate, DateTime endDate);
        Task<List<string>> GetRecommendationsAsync(DateTime startDate, DateTime endDate);

        // Data Export
        Task<byte[]> ExportSalesDataAsync(DateTime startDate, DateTime endDate, string format = "excel");
        Task<byte[]> ExportUserBehaviorDataAsync(DateTime startDate, DateTime endDate, string format = "excel");
        Task<byte[]> ExportPerformanceDataAsync(DateTime startDate, DateTime endDate, string format = "excel");
        Task<byte[]> ExportInventoryDataAsync(DateTime startDate, DateTime endDate, string format = "excel");

        // Real-time Analytics
        Task<Dictionary<string, object>> GetRealTimeMetricsAsync();
        Task<List<object>> GetRealTimeEventsAsync(int limit = 50);
        Task<Dictionary<string, object>> GetLiveDashboardAsync();

        // Custom Analytics
        Task<object> ExecuteCustomQueryAsync(string query, Dictionary<string, object> parameters);
        Task<List<object>> GetCustomMetricsAsync(List<string> metrics, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, object>> GetComparativeAnalysisAsync(DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End);

        // Analytics Configuration
        Task<Dictionary<string, object>> GetAnalyticsConfigurationAsync();
        Task<bool> UpdateAnalyticsConfigurationAsync(Dictionary<string, object> configuration);
        Task<bool> ResetAnalyticsDataAsync(DateTime startDate, DateTime endDate);
        Task<bool> BackupAnalyticsDataAsync(DateTime startDate, DateTime endDate);
    }
} 