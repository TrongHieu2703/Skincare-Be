using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(IInventoryRepository inventoryRepository, ILogger<InventoryService> logger)
        {
            _inventoryRepository = inventoryRepository;
            _logger = logger;
        }

        private InventoryDto MapToDto(Inventory inventory)
        {
            return new InventoryDto
            {
                Id = inventory.Id,
                ProductId = inventory.ProductId,
                BranchId = inventory.BranchId,
                Quantity = inventory.Quantity,
                BranchName = inventory.Branch?.Name,
                ProductName = inventory.Product?.Name
            };
        }

        public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
        {
            var inventories = await _inventoryRepository.GetAllInventoryAsync();
            return inventories.Select(MapToDto).ToList();
        }

        public async Task<InventoryDto> GetInventoryByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving inventory with ID {InventoryId}", id);
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null)
                throw new NotFoundException($"Inventory with ID {id} not found.");
            return MapToDto(inventory);
        }

        public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
        {
            var inventory = await _inventoryRepository.CreateInventoryAsync(createInventoryDto);
            return MapToDto(inventory);
        }

        public async Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null)
                throw new NotFoundException($"Inventory with ID {id} not found for update.");

            inventory.Quantity = updateInventoryDto.Quantity ?? inventory.Quantity;
            var updatedInventory = await _inventoryRepository.UpdateInventoryAsync(inventory);
            return MapToDto(updatedInventory);
        }

        public async Task DeleteInventoryAsync(int id)
        {
            _logger.LogInformation("Deleting inventory with ID {InventoryId}", id);
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(id);
            if (inventory == null)
                throw new NotFoundException($"Inventory with ID {id} not found for deletion.");
            await _inventoryRepository.DeleteInventoryAsync(id);
        }
    }
}
