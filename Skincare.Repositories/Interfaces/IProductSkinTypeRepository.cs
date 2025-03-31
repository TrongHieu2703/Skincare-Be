using Skincare.BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IProductSkinTypeRepository
    {
        Task<IEnumerable<ProductSkinType>> GetAllAsync();
        Task<ProductSkinType> GetByIdAsync(int id);
        Task<IEnumerable<ProductSkinType>> GetByProductIdAsync(int productId);
        Task<IEnumerable<ProductSkinType>> GetBySkinTypeIdAsync(int skinTypeId);
        Task<ProductSkinType> CreateAsync(ProductSkinType productSkinType);
        Task<ProductSkinType> UpdateAsync(ProductSkinType productSkinType);
        Task DeleteAsync(int id);
        Task DeleteByProductIdAsync(int productId);
    }
} 