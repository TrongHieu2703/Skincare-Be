using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.DTOs;

namespace Skincare.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> SearchProductsAsync(string keyword);
        Task<IEnumerable<Product>> FilterProductsAsync(string category, bool? inStock, decimal? minPrice, decimal? maxPrice);
        Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId); 
        Task<Product> CreateProductAsync(CreateProductDto createProductDto); 
        Task<Product> UpdateProductAsync(int id, UpdateProductDto updateProductDto); 
        Task DeleteProductAsync(int id);
    }
}
