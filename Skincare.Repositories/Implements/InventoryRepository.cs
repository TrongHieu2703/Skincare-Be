using Skincare.BusinessObjects.DTOs;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Skincare.BusinessObjects.Entities;
using System;

public class InventoryRepository : IInventoryRepository
{
    private readonly SWP391Context _context;

    public InventoryRepository(SWP391Context context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryDto>> GetAllInventoriesAsync()
    {
        return await _context.Inventories
            .Select(i => new InventoryDto
            {
                InventoryId = i.Id, 
                ProductId = i.ProductId,
                BranchId = i.BranchId,
                Quantity = i.Quantity,
            })
            .ToListAsync();
    }

    public async Task<InventoryDto> GetInventoryByIdAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        return inventory == null ? null : new InventoryDto
        {
            InventoryId = inventory.Id, 
            ProductId = inventory.ProductId,
            BranchId = inventory.BranchId,
            Quantity = inventory.Quantity,
        };
    }

    public async Task<InventoryDto> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
    {
        var inventory = new Inventory
        {
            ProductId = createInventoryDto.ProductId,
            BranchId = createInventoryDto.BranchId,
            Quantity = createInventoryDto.Quantity,
        };

        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();

        return new InventoryDto
        {
            InventoryId = inventory.Id,
            ProductId = inventory.ProductId,
            BranchId = inventory.BranchId,
            Quantity = inventory.Quantity,
        };
    }

    public async Task<InventoryDto> UpdateInventoryAsync(int id, UpdateInventoryDto updateInventoryDto)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory == null) return null;

        inventory.Quantity = updateInventoryDto.Quantity;

        await _context.SaveChangesAsync();

        return new InventoryDto
        {
            InventoryId = inventory.Id,
            ProductId = inventory.ProductId,
            BranchId = inventory.BranchId,
            Quantity = inventory.Quantity,
        };
    }

    public async Task DeleteInventoryAsync(int id)
    {
        var inventory = await _context.Inventories.FindAsync(id);
        if (inventory != null)
        {
            _context.Inventories.Remove(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
