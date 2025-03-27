using System.Collections.Generic;
using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface IProductTypeRepository
    {
        Task<IEnumerable<ProductType>> GetAllAsync();
        Task<ProductType> GetByIdAsync(int id);
    }
} 