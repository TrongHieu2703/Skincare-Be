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
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetByTypeAsync(int productTypeId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
        Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice);
        
        // Thêm phương thức mới hỗ trợ upload ảnh
        Task<ProductDto> CreateProductWithImageAsync(CreateProductDto createProductDto, IFormFile image);
        Task<ProductDto> UpdateProductWithImageAsync(int id, UpdateProductDto updateProductDto, IFormFile image);
        
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task DeleteProductAsync(int id);

        // So sánh sản phẩm: Nhận CompareRequestDto và trả về danh sách sản phẩm để so sánh.
        Task<IEnumerable<ProductDto>> CompareProductsAsync(CompareRequestDto compareRequestDto);

        Task<IEnumerable<ProductDto>> GetProductsBySkinTypeAsync(int skinTypeId);
    }
}
