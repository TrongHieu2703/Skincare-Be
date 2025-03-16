using Skincare.BusinessObjects.Entities;

namespace Skincare.Repositories.Interfaces
{
    public interface ICartItemRepository
    {
        Task<CartItem> GetCartItemByIdAsync(int id);
        Task<bool> UpdateCartItemAsync(int cartItemId, int quantity);
        Task<bool> DeleteCartItemAsync(int cartItemId);
    
    }
}
