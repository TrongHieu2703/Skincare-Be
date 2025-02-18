using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<Cart>> GetAllCartsAsync();
        Task<Cart> GetCartByIdAsync(int cartId);
        Task<IEnumerable<Cart>> GetCartsByUserIdAsync(int userId);
        Task<Cart> AddCartAsync(Cart cart);
        Task UpdateCartAsync(Cart cart);
        Task DeleteCartAsync(int cartId);
    }
}
