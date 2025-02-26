using System.Collections.Generic;
using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(int pageNumber, int pageSize);
        Task<Product> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetByTypeAsync(int productTypeId);
        Task<Product> CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }

}