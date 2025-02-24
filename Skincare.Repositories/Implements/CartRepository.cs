using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Skincare.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly SWP391Context _context;
        private readonly ILogger<CartRepository> _logger;

        public CartRepository(SWP391Context context, ILogger<CartRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Cart>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            try
            {
                return await _context.Carts
                    .AsNoTracking()
                    .Include(c => c.Product)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all carts");
                throw;
            }
        }


        public async Task<Cart> GetCartByIdAsync(int cartId)
        {
            try
            {
                return await _context.Carts.Include(c => c.Product)
                                           .FirstOrDefaultAsync(c => c.CartId == cartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cart with ID {cartId}");
                throw;
            }
        }

        public async Task<IEnumerable<Cart>> GetCartsByUserIdAsync(int userId)
        {
            try
            {
                return await _context.Carts.Where(c => c.UserId == userId)
                                           .Include(c => c.Product)
                                           .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching carts for user ID {userId}");
                throw;
            }
        }

        public async Task<Cart> AddCartAsync(Cart cart)
        {
            try
            {
                await _context.Carts.AddAsync(cart);
                await _context.SaveChangesAsync();
                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cart");
                throw;
            }
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            try
            {
                _context.Carts.Update(cart);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart with ID {cart.CartId}");
                throw;
            }
        }

        public async Task DeleteCartAsync(int cartId)
        {
            try
            {
                var cart = await _context.Carts.FindAsync(cartId);
                if (cart != null)
                {
                    _context.Carts.Remove(cart);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart with ID {cartId}");
                throw;
            }
        }
    }
}