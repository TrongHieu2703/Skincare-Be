using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<int> GetTotalProductsCountAsync();
        Task<(IEnumerable<Product> Products, int TotalCount)> GetAllProductsWithPaginationAsync(int pageNumber, int pageSize);
        Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword);
        Task<IEnumerable<Product>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
        Task<IEnumerable<Product>> GetProductsBySkinTypeAsync(int skinTypeId);
        Task<int> GetTotalInventoryQuantityAsync(int productId);
        
        // New methods for filtering and pagination
        Task<IQueryable<Product>> GetFilteredProductsQueryAsync(
            int? skinTypeId, 
            int? productTypeId, 
            int? branchId, 
            decimal? minPrice, 
            decimal? maxPrice);
            
        Task<int> GetCountFromQueryAsync(IQueryable<Product> query);
        
        Task<IEnumerable<Product>> GetPaginatedProductsFromQueryAsync(
            IQueryable<Product> query, 
            int pageNumber, 
            int pageSize);
            
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsByTypeWithPaginationAsync(
            int productTypeId, 
            int pageNumber, 
            int pageSize);
            
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsByBranchWithPaginationAsync(
            int branchId, 
            int pageNumber, 
            int pageSize);
            
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsBySkinTypeWithPaginationAsync(
            int skinTypeId, 
            int pageNumber, 
            int pageSize);
    }
}
