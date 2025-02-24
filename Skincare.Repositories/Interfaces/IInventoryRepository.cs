using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IInventoryRepository
{
    Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync();
    Task<InventoryDto> GetInventoryByIdAsync(int id);
    Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto);
    Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto);
    Task DeleteInventoryAsync(int id);
}
