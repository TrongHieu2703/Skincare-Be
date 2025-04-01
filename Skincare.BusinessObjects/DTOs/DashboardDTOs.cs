using System;
using System.Collections.Generic;

namespace Skincare.BusinessObjects.DTOs
{
    // DTO containing all dashboard statistics
    public class DashboardStatsDto
    {
        public ProductStatsDto ProductStats { get; set; }
        public CustomerStatsDto CustomerStats { get; set; }
        public OrderStatsDto OrderStats { get; set; }
        public RevenueStatsDto RevenueStats { get; set; }
        public List<TopProductDto> BestSellers { get; set; }
        public List<TopProductDto> TopRated { get; set; }
        public TimeRangeDto TimeRange { get; set; }
    }

    // DTO for time range selection
    public class TimeRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RangeType { get; set; } // "24h", "7d", "30d", "12m", "all"
    }

    // DTO for product statistics
    public class ProductStatsDto
    {
        public int TotalProducts { get; set; }
        public int InStockProducts { get; set; }
        public int LowStockProducts { get; set; } // Products with stock <= 10
        public int TotalProductsSold { get; set; }
        public List<ProductLowStockDto> LowStockWarnings { get; set; }
    }

    // DTO for product low stock warning
    public class ProductLowStockDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentStock { get; set; }
        public string Image { get; set; }
    }

    // DTO for customer statistics
    public class CustomerStatsDto
    {
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; } // New customers in the selected time range
    }

    // DTO for order statistics
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ConfirmedOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public List<ChartDataPointDto> OrdersByTime { get; set; }
        public List<OrderStatusDistributionDto> OrderStatusDistribution { get; set; }
    }

    // DTO for order status distribution (for pie charts)
    public class OrderStatusDistributionDto
    {
        public string Status { get; set; }
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    // DTO for revenue statistics
    public class RevenueStatsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<ChartDataPointDto> RevenueByTime { get; set; }
    }

    // DTO for chart data points
    public class ChartDataPointDto
    {
        public string Label { get; set; } // Date, month, or year label
        public decimal Value { get; set; } // The value for that time point
    }

    // DTO for top products (bestsellers or top rated)
    public class TopProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int QuantitySold { get; set; }
        public decimal? AverageRating { get; set; }
        public int ReviewsCount { get; set; }
    }

    // DTO for dashboard request parameters
    public class DashboardRequestDto
    {
        public string TimeRange { get; set; } = "30d"; // Default to 30 days
        public DateTime? CustomStartDate { get; set; }
        public DateTime? CustomEndDate { get; set; }
    }
} 