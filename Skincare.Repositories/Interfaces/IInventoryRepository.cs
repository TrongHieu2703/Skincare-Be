using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryByIdAsync(int id);
        Task<bool> CheckStockAvailability(int productId, int quantity);
        Task<Inventory> UpdateInventoryAsync(Inventory inventory);
        Task<Inventory> CreateInventoryAsync(CreateInventoryDto createInventoryDto);
        Task DeleteInventoryAsync(int id);
    }
}
