using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly SWP391Context _context;
        public InventoryRepository(SWP391Context context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            return await _context.Inventories.Include(i => i.Product)
                                             .Include(i => i.Branch)
                                             .ToListAsync();
        }

        public async Task<Inventory> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories.Include(i => i.Product)
                                             .Include(i => i.Branch)
                                             .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> CheckStockAvailability(int productId, int quantity)
        {
            // Tổng số lượng tồn kho cho sản phẩm này
            int stock = await _context.Inventories.Where(i => i.ProductId == productId)
                                                  .SumAsync(i => i.Quantity) ?? 0;
            return stock >= quantity;
        }

        public async Task<Inventory> CreateInventoryAsync(CreateInventoryDto createInventoryDto)
        {
            var inventory = new Inventory
            {
                ProductId = createInventoryDto.ProductId,
                BranchId = createInventoryDto.BranchId,
                Quantity = createInventoryDto.Quantity
            };
            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<Inventory> UpdateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
            return inventory;
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
}
