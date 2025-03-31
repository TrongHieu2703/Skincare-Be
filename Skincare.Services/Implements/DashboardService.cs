using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(
            IDashboardRepository dashboardRepository,
            ILogger<DashboardService> logger)
        {
            _dashboardRepository = dashboardRepository;
            _logger = logger;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(DashboardRequestDto request)
        {
            try
            {
                // Calculate time range based on request
                var timeRange = CalculateTimeRange(request);
                
                // Get statistics sequentially
                var productStats = await GetProductStatsAsync(timeRange.StartDate, timeRange.EndDate);
                var customerStats = await GetCustomerStatsAsync(timeRange.StartDate, timeRange.EndDate);
                var orderStats = await GetOrderStatsAsync(timeRange.StartDate, timeRange.EndDate, GetAppropriateInterval(timeRange.RangeType));
                var revenueStats = await GetRevenueStatsAsync(timeRange.StartDate, timeRange.EndDate, GetAppropriateInterval(timeRange.RangeType));
                var bestSellers = await GetBestSellingProductsAsync(timeRange.StartDate, timeRange.EndDate);
                var topRated = await GetTopRatedProductsAsync();

                // Combine results into a single dashboard statistics object
                return new DashboardStatsDto
                {
                    ProductStats = productStats,
                    CustomerStats = customerStats,
                    OrderStats = orderStats,
                    RevenueStats = revenueStats,
                    BestSellers = bestSellers.ToList(),
                    TopRated = topRated.ToList(),
                    TimeRange = timeRange
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard statistics");
                throw;
            }
        }

        public TimeRangeDto CalculateTimeRange(DashboardRequestDto request)
        {
            DateTime endDate = DateTime.UtcNow;
            DateTime startDate;
            string rangeType = request.TimeRange?.ToLower() ?? "30d";

            // If custom dates are provided, use them
            if (request.CustomStartDate.HasValue && request.CustomEndDate.HasValue)
            {
                startDate = request.CustomStartDate.Value;
                endDate = request.CustomEndDate.Value;
                rangeType = "custom";
            }
            else
            {
                // Calculate start date based on time range
                switch (rangeType)
                {
                    case "24h":
                        startDate = endDate.AddHours(-24);
                        break;
                    case "7d":
                        startDate = endDate.AddDays(-7);
                        break;
                    case "30d":
                        startDate = endDate.AddDays(-30);
                        break;
                    case "12m":
                        startDate = endDate.AddMonths(-12);
                        break;
                    case "all":
                        startDate = new DateTime(2000, 1, 1); // A date far in the past
                        break;
                    default:
                        startDate = endDate.AddDays(-30); // Default to 30 days
                        rangeType = "30d";
                        break;
                }
            }

            return new TimeRangeDto
            {
                StartDate = startDate,
                EndDate = endDate,
                RangeType = rangeType
            };
        }

        private string GetAppropriateInterval(string rangeType)
        {
            switch (rangeType)
            {
                case "24h":
                    return "hour";
                case "7d":
                case "30d":
                    return "day";
                case "12m":
                    return "month";
                case "all":
                    return "year";
                default:
                    return "day";
            }
        }

        public async Task<ProductStatsDto> GetProductStatsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Fetch all product statistics data
                var totalProducts = await _dashboardRepository.GetTotalProductsCountAsync();
                var inStockProducts = await _dashboardRepository.GetInStockProductsCountAsync();
                var lowStockProducts = await _dashboardRepository.GetLowStockProductsCountAsync();
                var lowStockProductsList = await _dashboardRepository.GetLowStockProductsAsync();
                var totalProductsSold = await _dashboardRepository.GetTotalProductsSoldAsync(startDate, endDate);

                // Map low stock products to DTOs
                var lowStockWarnings = lowStockProductsList.Select(p => new ProductLowStockDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CurrentStock = p.Stock ?? 0,
                    Image = p.Image
                }).ToList();

                return new ProductStatsDto
                {
                    TotalProducts = totalProducts,
                    InStockProducts = inStockProducts,
                    LowStockProducts = lowStockProducts,
                    TotalProductsSold = totalProductsSold,
                    LowStockWarnings = lowStockWarnings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product statistics");
                throw;
            }
        }

        public async Task<CustomerStatsDto> GetCustomerStatsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var totalCustomers = await _dashboardRepository.GetTotalCustomersCountAsync();
                var newCustomers = await _dashboardRepository.GetNewCustomersCountAsync(startDate, endDate);

                return new CustomerStatsDto
                {
                    TotalCustomers = totalCustomers,
                    NewCustomers = newCustomers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer statistics");
                throw;
            }
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync(DateTime startDate, DateTime endDate, string interval = "day")
        {
            try
            {
                // Get order counts by status
                var totalOrders = await _dashboardRepository.GetTotalOrdersCountAsync(startDate, endDate);
                var pendingOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Pending", startDate, endDate);
                var confirmedOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Confirmed", startDate, endDate);
                var processingOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Processing", startDate, endDate);
                var shippedOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Shipped", startDate, endDate);
                var deliveredOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Delivered", startDate, endDate);
                var cancelledOrders = await _dashboardRepository.GetOrdersCountByStatusAsync("Cancelled", startDate, endDate);

                // Get order status distribution for pie chart
                var statusDistribution = await _dashboardRepository.GetOrderStatusDistributionAsync(startDate, endDate);
                var orderStatusDistribution = statusDistribution.Select(x => new OrderStatusDistributionDto
                {
                    Status = x.Key,
                    Count = x.Value,
                    Percentage = totalOrders > 0 ? Math.Round((decimal)x.Value / totalOrders * 100, 2) : 0
                }).ToList();

                // Get orders by time for line chart
                var ordersByTime = await _dashboardRepository.GetOrdersByTimeAsync(startDate, endDate, interval);
                var ordersByTimeChart = ordersByTime.Select(x => new ChartDataPointDto
                {
                    Label = FormatDateLabel(x.Date, interval),
                    Value = x.Count
                }).ToList();

                return new OrderStatsDto
                {
                    TotalOrders = totalOrders,
                    PendingOrders = pendingOrders,
                    ConfirmedOrders = confirmedOrders,
                    ProcessingOrders = processingOrders,
                    ShippedOrders = shippedOrders,
                    DeliveredOrders = deliveredOrders,
                    CancelledOrders = cancelledOrders,
                    OrdersByTime = ordersByTimeChart,
                    OrderStatusDistribution = orderStatusDistribution
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order statistics");
                throw;
            }
        }

        public async Task<RevenueStatsDto> GetRevenueStatsAsync(DateTime startDate, DateTime endDate, string interval = "day")
        {
            try
            {
                var totalRevenue = await _dashboardRepository.GetTotalRevenueAsync(startDate, endDate);
                var averageOrderValue = await _dashboardRepository.GetAverageOrderValueAsync(startDate, endDate);
                var revenueByTime = await _dashboardRepository.GetRevenueByTimeAsync(startDate, endDate, interval);

                // Convert to chart data points
                var revenueByTimeChart = revenueByTime.Select(x => new ChartDataPointDto
                {
                    Label = FormatDateLabel(x.Date, interval),
                    Value = x.Revenue
                }).ToList();

                return new RevenueStatsDto
                {
                    TotalRevenue = totalRevenue,
                    AverageOrderValue = averageOrderValue,
                    RevenueByTime = revenueByTimeChart
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting revenue statistics");
                throw;
            }
        }

        public async Task<TopProductDto[]> GetBestSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 5)
        {
            try
            {
                var bestSellers = await _dashboardRepository.GetBestSellingProductsAsync(startDate, endDate, limit);

                return bestSellers.Select(x => new TopProductDto
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Image = x.Product.Image,
                    Price = x.Product.Price,
                    QuantitySold = x.QuantitySold,
                    AverageRating = CalculateAverageRating(x.Product),
                    ReviewsCount = x.Product.Reviews?.Count ?? 0
                }).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting best selling products");
                throw;
            }
        }

        public async Task<TopProductDto[]> GetTopRatedProductsAsync(int limit = 5)
        {
            try
            {
                var topRated = await _dashboardRepository.GetTopRatedProductsAsync(limit);

                return topRated.Select(x => new TopProductDto
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Image = x.Product.Image,
                    Price = x.Product.Price,
                    QuantitySold = 0, // We don't have this information here
                    AverageRating = x.Rating,
                    ReviewsCount = x.Product.Reviews?.Count ?? 0
                }).ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top rated products");
                throw;
            }
        }

        private string FormatDateLabel(DateTime date, string interval)
        {
            switch (interval)
            {
                case "hour":
                    return date.ToString("HH:00 dd/MM");
                case "day":
                    return date.ToString("dd/MM/yyyy");
                case "month":
                    return date.ToString("MM/yyyy");
                case "year":
                    return date.ToString("yyyy");
                default:
                    return date.ToString("dd/MM/yyyy");
            }
        }

        private decimal? CalculateAverageRating(Product product)
        {
            if (product.Reviews == null || !product.Reviews.Any())
                return null;

            var validRatings = product.Reviews.Where(r => r.Rating.HasValue).Select(r => r.Rating.Value);
            if (!validRatings.Any())
                return null;

            return Math.Round((decimal)validRatings.Average(), 1);
        }
    }
} 