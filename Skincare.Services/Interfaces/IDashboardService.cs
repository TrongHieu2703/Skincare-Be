using Skincare.BusinessObjects.DTOs;
using System;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Get all dashboard statistics for a specific time range
        /// </summary>
        Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardRequestDto request);
        
        /// <summary>
        /// Get product statistics for the dashboard
        /// </summary>
        Task<ProductStatsDto> GetProductStatsAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Get customer statistics for the dashboard
        /// </summary>
        Task<CustomerStatsDto> GetCustomerStatsAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Get order statistics for the dashboard
        /// </summary>
        Task<OrderStatsDto> GetOrderStatsAsync(DateTime startDate, DateTime endDate, string interval = "day");
        
        /// <summary>
        /// Get revenue statistics for the dashboard
        /// </summary>
        Task<RevenueStatsDto> GetRevenueStatsAsync(DateTime startDate, DateTime endDate, string interval = "day");
        
        /// <summary>
        /// Get top selling products for the dashboard
        /// </summary>
        Task<TopProductDto[]> GetBestSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 5);
        
        /// <summary>
        /// Get top rated products for the dashboard
        /// </summary>
        Task<TopProductDto[]> GetTopRatedProductsAsync(int limit = 5);
        
        /// <summary>
        /// Calculate appropriate date range based on request parameters
        /// </summary>
        TimeRangeDto CalculateTimeRange(DashboardRequestDto request);
    }
} 