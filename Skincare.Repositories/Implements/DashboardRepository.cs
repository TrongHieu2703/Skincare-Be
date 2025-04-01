using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<DashboardRepository> _logger;

        public DashboardRepository(SWP391Context context, ILogger<DashboardRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Product statistics
        public async Task<int> GetTotalProductsCountAsync()
        {
            try
            {
                return await _context.Products.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total products count");
                throw;
            }
        }

        public async Task<int> GetInStockProductsCountAsync()
        {
            try
            {
                return await _context.Products
                    .Where(p => p.Stock > 0)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-stock products count");
                throw;
            }
        }

        public async Task<int> GetLowStockProductsCountAsync(int threshold = 10)
        {
            try
            {
                return await _context.Products
                    .Where(p => p.Stock > 0 && p.Stock <= threshold)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting low-stock products count (threshold: {threshold})");
                throw;
            }
        }

        public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10, int limit = 10)
        {
            try
            {
                return await _context.Products
                    .Where(p => p.Stock > 0 && p.Stock <= threshold)
                    .OrderBy(p => p.Stock)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting low-stock products (threshold: {threshold}, limit: {limit})");
                throw;
            }
        }

        public async Task<int> GetTotalProductsSoldAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Sum of order item quantities for delivered orders within the date range
                return await _context.OrderItems
                    .Where(oi => oi.Order.Status == "Delivered" &&
                                oi.Order.UpdatedAt >= startDate &&
                                oi.Order.UpdatedAt <= endDate)
                    .SumAsync(oi => oi.ItemQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total products sold (period: {startDate} to {endDate})");
                throw;
            }
        }

        // Customer statistics
        public async Task<int> GetTotalCustomersCountAsync()
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.Role == "User")
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total customers count");
                throw;
            }
        }

        public async Task<int> GetNewCustomersCountAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Accounts
                    .Where(a => a.Role == "User" &&
                               a.CreatedAt >= startDate &&
                               a.CreatedAt <= endDate)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting new customers count (period: {startDate} to {endDate})");
                throw;
            }
        }

        // Order statistics
        public async Task<int> GetTotalOrdersCountAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.UpdatedAt >= startDate && o.UpdatedAt <= endDate)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total orders count (period: {startDate} to {endDate})");
                throw;
            }
        }

        public async Task<int> GetOrdersCountByStatusAsync(string status, DateTime startDate, DateTime endDate)
        {
            try
            {
                return await _context.Orders
                    .Where(o => o.Status == status &&
                               o.UpdatedAt >= startDate &&
                               o.UpdatedAt <= endDate)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting orders count for status {status} (period: {startDate} to {endDate})");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetOrderStatusDistributionAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var statusCounts = await _context.Orders
                    .Where(o => o.UpdatedAt >= startDate && o.UpdatedAt <= endDate)
                    .GroupBy(o => o.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                return statusCounts.ToDictionary(x => x.Status, x => x.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting order status distribution (period: {startDate} to {endDate})");
                throw;
            }
        }

        public async Task<List<(DateTime Date, int Count)>> GetOrdersByTimeAsync(DateTime startDate, DateTime endDate, string interval = "day")
        {
            try
            {
                // Fetch data to memory first
                var orders = await _context.Orders
                    .Where(o => o.UpdatedAt >= startDate && o.UpdatedAt <= endDate)
                    .ToListAsync();
                    
                // Fill in missing intervals for continuous timeline
                var result = new List<(DateTime Date, int Count)>();
                
                if (interval == "hour")
                {
                    // Cách xử lý dữ liệu theo giờ - giữ nguyên
                    var hourlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, o.UpdatedAt.Value.Day, o.UpdatedAt.Value.Hour, 0, 0))
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .OrderBy(x => x.Date)
                        .ToList();

                    result = hourlyData.Select(x => (x.Date, x.Count)).ToList();
                }
                else if (interval == "day")
                {
                    // Lấy danh sách tất cả các ngày trong khoảng thời gian
                    var allDates = new List<DateTime>();
                    for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
                    {
                        allDates.Add(dt);
                    }
                    
                    // Nhóm dữ liệu theo ngày
                    var dailyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, o.UpdatedAt.Value.Day))
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì đếm = 0
                    foreach (var date in allDates)
                    {
                        var count = dailyData.ContainsKey(date) ? dailyData[date] : 0;
                        result.Add((date, count));
                    }
                }
                else if (interval == "month")
                {
                    // Lấy danh sách tất cả các tháng trong khoảng thời gian
                    var allMonths = new List<DateTime>();
                    var startMonth = new DateTime(startDate.Year, startDate.Month, 1);
                    var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                    
                    for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
                    {
                        allMonths.Add(month);
                    }
                    
                    // Nhóm dữ liệu theo tháng
                    var monthlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, 1))
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì đếm = 0
                    foreach (var month in allMonths)
                    {
                        var count = monthlyData.ContainsKey(month) ? monthlyData[month] : 0;
                        result.Add((month, count));
                    }
                }
                else // year
                {
                    // Lấy danh sách tất cả các năm trong khoảng thời gian
                    var allYears = new List<DateTime>();
                    var startYear = new DateTime(startDate.Year, 1, 1);
                    var endYear = new DateTime(endDate.Year, 1, 1);
                    
                    for (var year = startYear; year <= endYear; year = year.AddYears(1))
                    {
                        allYears.Add(year);
                    }
                    
                    // Nhóm dữ liệu theo năm
                    var yearlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, 1, 1))
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì đếm = 0
                    foreach (var year in allYears)
                    {
                        var count = yearlyData.ContainsKey(year) ? yearlyData[year] : 0;
                        result.Add((year, count));
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting orders by time");
                throw;
            }
        }

        // Revenue statistics
        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Sum total amounts of delivered orders
                return await _context.Orders
                    .Where(o => o.Status == "Delivered" &&
                               o.UpdatedAt >= startDate &&
                               o.UpdatedAt <= endDate)
                    .SumAsync(o => o.TotalAmount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting total revenue (period: {startDate} to {endDate})");
                throw;
            }
        }

        public async Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Average total amount of delivered orders
                var averageValue = await _context.Orders
                    .Where(o => o.Status == "Delivered" &&
                               o.UpdatedAt >= startDate &&
                               o.UpdatedAt <= endDate)
                    .AverageAsync(o => (decimal?)o.TotalAmount);

                return averageValue ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting average order value (period: {startDate} to {endDate})");
                throw;
            }
        }

        public async Task<List<(DateTime Date, decimal Revenue)>> GetRevenueByTimeAsync(DateTime startDate, DateTime endDate, string interval = "day")
        {
            try
            {
                // Fetch data to memory first
                var orders = await _context.Orders
                    .Where(o => o.Status == "Delivered" &&
                               o.UpdatedAt >= startDate &&
                               o.UpdatedAt <= endDate)
                    .ToListAsync();
                    
                // Fill in missing intervals for continuous timeline
                var result = new List<(DateTime Date, decimal Revenue)>();
                
                if (interval == "hour")
                {
                    // Cách xử lý dữ liệu theo giờ - giữ nguyên
                    var hourlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, o.UpdatedAt.Value.Day, o.UpdatedAt.Value.Hour, 0, 0))
                        .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                        .OrderBy(x => x.Date)
                        .ToList();

                    result = hourlyData.Select(x => (x.Date, x.Revenue)).ToList();
                }
                else if (interval == "day")
                {
                    // Lấy danh sách tất cả các ngày trong khoảng thời gian
                    var allDates = new List<DateTime>();
                    for (var dt = startDate.Date; dt <= endDate.Date; dt = dt.AddDays(1))
                    {
                        allDates.Add(dt);
                    }
                    
                    // Nhóm dữ liệu theo ngày
                    var dailyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, o.UpdatedAt.Value.Day))
                        .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì doanh thu = 0
                    foreach (var date in allDates)
                    {
                        var revenue = dailyData.ContainsKey(date) ? dailyData[date] : 0;
                        result.Add((date, revenue));
                    }
                }
                else if (interval == "month")
                {
                    // Lấy danh sách tất cả các tháng trong khoảng thời gian
                    var allMonths = new List<DateTime>();
                    var startMonth = new DateTime(startDate.Year, startDate.Month, 1);
                    var endMonth = new DateTime(endDate.Year, endDate.Month, 1);
                    
                    for (var month = startMonth; month <= endMonth; month = month.AddMonths(1))
                    {
                        allMonths.Add(month);
                    }
                    
                    // Nhóm dữ liệu theo tháng
                    var monthlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, o.UpdatedAt.Value.Month, 1))
                        .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì doanh thu = 0
                    foreach (var month in allMonths)
                    {
                        var revenue = monthlyData.ContainsKey(month) ? monthlyData[month] : 0;
                        result.Add((month, revenue));
                    }
                }
                else // year
                {
                    // Lấy danh sách tất cả các năm trong khoảng thời gian
                    var allYears = new List<DateTime>();
                    var startYear = new DateTime(startDate.Year, 1, 1);
                    var endYear = new DateTime(endDate.Year, 1, 1);
                    
                    for (var year = startYear; year <= endYear; year = year.AddYears(1))
                    {
                        allYears.Add(year);
                    }
                    
                    // Nhóm dữ liệu theo năm
                    var yearlyData = orders
                        .Where(o => o.UpdatedAt.HasValue)
                        .GroupBy(o => new DateTime(o.UpdatedAt.Value.Year, 1, 1))
                        .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
                    
                    // Tạo kết quả đầy đủ, nếu không có dữ liệu thì doanh thu = 0
                    foreach (var year in allYears)
                    {
                        var revenue = yearlyData.ContainsKey(year) ? yearlyData[year] : 0;
                        result.Add((year, revenue));
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting revenue by time");
                throw;
            }
        }

        // Top products
        public async Task<List<(Product Product, int QuantitySold)>> GetBestSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 5)
        {
            try
            {
                // Group by product ID and sum quantities from delivered orders
                var topProducts = await _context.OrderItems
                    .Where(oi => oi.Order.Status == "Delivered" &&
                                oi.Order.UpdatedAt >= startDate &&
                                oi.Order.UpdatedAt <= endDate)
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        QuantitySold = g.Sum(oi => oi.ItemQuantity)
                    })
                    .OrderByDescending(x => x.QuantitySold)
                    .Take(limit)
                    .ToListAsync();

                var result = new List<(Product Product, int QuantitySold)>();

                // For each top product, get the full product entity
                foreach (var item in topProducts)
                {
                    var product = await _context.Products
                        .Include(p => p.ProductBrand)
                        .Include(p => p.ProductType)
                        .Include(p => p.Reviews)
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);

                    if (product != null)
                    {
                        result.Add((product, item.QuantitySold));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting best selling products (period: {startDate} to {endDate}, limit: {limit})");
                throw;
            }
        }

        public async Task<List<(Product Product, decimal Rating)>> GetTopRatedProductsAsync(int limit = 5)
        {
            try
            {
                // Get products with ratings
                var productsWithRatings = await _context.Products
                    .Include(p => p.ProductBrand)
                    .Include(p => p.ProductType)
                    .Include(p => p.Reviews)
                    .Where(p => p.Reviews.Any(r => r.Rating.HasValue)) // Only include products with ratings
                    .Select(p => new
                    {
                        Product = p,
                        AverageRating = p.Reviews
                            .Where(r => r.Rating.HasValue)
                            .Average(r => r.Rating.Value)
                    })
                    .OrderByDescending(x => x.AverageRating)
                    .Take(limit)
                    .ToListAsync();

                return productsWithRatings
                    .Select(x => (x.Product, (decimal)x.AverageRating))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting top rated products (limit: {limit})");
                throw;
            }
        }
    }
} 