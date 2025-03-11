using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<IEnumerable<ProductDto>> GetByTypeAsync(int productTypeId);
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string keyword);
        Task<IEnumerable<ProductDto>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice);
        Task<ProductDto> CreateProductAsync(CreateProductDto createProductDto);
        Task<ProductDto> UpdateProductAsync(int id, UpdateProductDto updateProductDto);
        Task DeleteProductAsync(int id);

        // So sánh sản phẩm: Nhận CompareRequestDto và trả về danh sách sản phẩm để so sánh.
        Task<IEnumerable<ProductDto>> CompareProductsAsync(CompareRequestDto compareRequestDto);
    }
}
