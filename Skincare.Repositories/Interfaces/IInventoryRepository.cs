using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllAsync();
        Task<Inventory> GetByIdAsync(int id);
        Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId);
        Task<Inventory> CreateAsync(Inventory inventory);
        Task<Inventory> UpdateAsync(Inventory inventory);
        Task DeleteAsync(int id);
    }
}
