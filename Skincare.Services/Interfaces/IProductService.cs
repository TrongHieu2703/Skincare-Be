using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Skincare.Services.Interfaces
{
    public interface IProductService
    {
        //Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetAllProductsAsync(int pageNumber, int pageSize);
        
        Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsWithFiltersAsync(
            int pageNumber, 
            int pageSize, 
            int? skinTypeId = null, 
            int? productTypeId = null, 
            int? branchId = null,
            decimal? minPrice = null, 
            decimal? maxPrice = null, 
            decimal? minRating = null, 
            decimal? maxRating = null, 
            string sortBy = null);
        
        Task<ProductDto> GetProductByIdAsync(int id);
        
        Task<IEnumerable<ProductDto>> GetByTypeAsync(int productTypeId);
        
        Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsByTypeWithPaginationAsync(
            int productTypeId, 
            int pageNumber, 
            int pageSize);
        
        Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsByBranchWithPaginationAsync(
            int branchId, 
            int pageNumber, 
            int pageSize);
        
        Task<(IEnumerable<ProductDto> Products, int TotalPages, int TotalItems)> GetProductsBySkinTypeWithPaginationAsync(
            int skinTypeId, 
            int pageNumber, 
            int pageSize);
        
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
        
        Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice);
        
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        
        Task DeleteProductAsync(int id);
        
        Task<IEnumerable<ProductDto>> CompareProductsAsync(CompareRequestDto compareRequestDto);
        
        Task<ProductDto> CreateProductWithImageAsync(CreateProductDto createProductDto, IFormFile image);
        
        Task<ProductDto> UpdateProductWithImageAsync(int id, UpdateProductDto updateProductDto, IFormFile image);
        
        Task<IEnumerable<ProductDto>> GetProductsBySkinTypeAsync(int skinTypeId);
    }
}
