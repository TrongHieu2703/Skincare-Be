using Skincare.BusinessObjects.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Repositories.Interfaces
{
    public interface ICartRepository
    {
        // Các phương thức quản lý Cart
        Task<IEnumerable<Cart>> GetAllCartsAsync(int pageNumber, int pageSize);
        Task<Cart> GetCartByIdAsync(int id);
        Task<Cart> GetCartsByUserIdAsync(int userId);
        //Task<Cart> GetCartByUserAndProductAsync(int userId, int productId); 
        Task<Cart> AddCartAsync(Cart cart);
        Task<Cart> UpdateCartAsync(Cart cart);
        Task<bool> DeleteCartAsync(int id);

        // Các phương thức quản lý CartItem
        Task<CartItem> GetCartItemAsync(int cartId, int productId);
        Task<CartItem> AddCartItemAsync(CartItem cartItem);
        Task<CartItem> UpdateCartItemAsync(CartItem cartItem);
        Task<bool> DeleteCartItemAsync(int cartId, int productId);
        Task<bool> ClearCartAsync(int cartId);
    }
}
