using Microsoft.EntityFrameworkCore;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Context;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Repositories.Implements
{
    public class CartRepository : ICartRepository
    {
        private readonly SWP391Context _context;

        public CartRepository(SWP391Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Cart>> GetAllCartsAsync()
        {
            return await _context.Carts.Include(c => c.Product).ToListAsync();
        }

        public async Task<Cart> GetCartByIdAsync(int cartId)
        {
            return await _context.Carts.Include(c => c.Product)
                                       .FirstOrDefaultAsync(c => c.CartId == cartId);
        }

        public async Task<IEnumerable<Cart>> GetCartsByUserIdAsync(int userId)
        {
            return await _context.Carts.Where(c => c.UserId == userId)
                                       .Include(c => c.Product)
                                       .ToListAsync();
        }

        public async Task<Cart> AddCartAsync(Cart cart)
        {
            await _context.Carts.AddAsync(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            _context.Carts.Update(cart);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCartAsync(int cartId)
        {
            var cart = await _context.Carts.FindAsync(cartId);
            if (cart != null)
            {
                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
            }
        }
    }
}
