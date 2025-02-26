using Skincare.BusinessObjects.DTOs;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _inventoryRepository;

    public InventoryService(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
    {
        return await _inventoryRepository.GetAllInventoriesAsync();
    }

    public async Task<InventoryDto> GetInventoryByIdAsync(int id)
    {
        return await _inventoryRepository.GetInventoryByIdAsync(id);
    }

    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
    {
        return await _inventoryRepository.CreateInventoryAsync(createInventoryDto);
    }

    public async Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto)
    {
        return await _inventoryRepository.UpdateInventoryAsync(id, updateInventoryDto);
    }

    public async Task DeleteInventoryAsync(int id)
    {
        await _inventoryRepository.DeleteInventoryAsync(id);
    }
}
