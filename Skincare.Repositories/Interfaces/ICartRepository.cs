using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<IEnumerable<Cart>> GetAllCartsAsync();
        Task<Cart> GetCartByIdAsync(int cartId);
        Task<IEnumerable<Cart>> GetCartsByUserIdAsync(int userId);
        Task<Cart> AddCartAsync(Cart cart);
        Task UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(int cartId);
    }
}
