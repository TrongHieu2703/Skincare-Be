using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<InventoryRepository> _logger;

        public InventoryRepository(SWP391Context context, ILogger<InventoryRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            try
            {
                return await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Branch)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all inventories");
                throw;
            }
        }

        public async Task<Inventory> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Branch)
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting inventory with ID {id}");
                throw;
            }
        }

        public async Task<bool> CheckStockAvailability(int productId, int quantity)
        {
            // Tổng số lượng tồn kho cho sản phẩm này
            int stock = await _context.Inventories.Where(i => i.ProductId == productId)
                                                  .SumAsync(i => i.Quantity) ?? 0;
            return stock >= quantity;
        }

        public async Task<Inventory> CreateAsync(Inventory inventory)
        {
            try
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating inventory");
                throw;
            }
        }

        public async Task<Inventory> UpdateAsync(Inventory inventory)
        {
            try
            {
                _context.Entry(inventory).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while updating inventory with ID {inventory.Id}");
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var inventory = await _context.Inventories.FindAsync(id);
                if (inventory != null)
                {
                    _context.Inventories.Remove(inventory);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while deleting inventory with ID {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Inventory>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _context.Inventories
                    .Include(i => i.Product)
                    .Include(i => i.Branch)
                    .Where(i => i.ProductId == productId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred while getting inventories for product ID {productId}");
                throw;
            }
        }
    }
}
