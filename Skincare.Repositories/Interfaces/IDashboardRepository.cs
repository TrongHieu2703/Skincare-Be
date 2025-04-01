using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        // Product statistics
        Task<int> GetTotalProductsCountAsync();
        Task<int> GetInStockProductsCountAsync();
        Task<int> GetLowStockProductsCountAsync(int threshold = 10);
        Task<List<Product>> GetLowStockProductsAsync(int threshold = 10, int limit = 10);
        Task<int> GetTotalProductsSoldAsync(DateTime startDate, DateTime endDate);

        // Customer statistics
        Task<int> GetTotalCustomersCountAsync();
        Task<int> GetNewCustomersCountAsync(DateTime startDate, DateTime endDate);

        // Order statistics
        Task<int> GetTotalOrdersCountAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrdersCountByStatusAsync(string status, DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderStatusDistributionAsync(DateTime startDate, DateTime endDate);
        Task<List<(DateTime Date, int Count)>> GetOrdersByTimeAsync(DateTime startDate, DateTime endDate, string interval = "day");

        // Revenue statistics
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetAverageOrderValueAsync(DateTime startDate, DateTime endDate);
        Task<List<(DateTime Date, decimal Revenue)>> GetRevenueByTimeAsync(DateTime startDate, DateTime endDate, string interval = "day");

        // Top products
        Task<List<(Product Product, int QuantitySold)>> GetBestSellingProductsAsync(DateTime startDate, DateTime endDate, int limit = 5);
        Task<List<(Product Product, decimal Rating)>> GetTopRatedProductsAsync(int limit = 5);
    }
} 