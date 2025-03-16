using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDTO>> GetAllCartsAsync(int pageNumber, int pageSize);
        Task<CartDTO> GetCartByIdAsync(int id);
        Task<CartDTO> GetCartByUserIdAsync(int userId);
        Task<CartDTO> AddCartAsync(AddToCartDTO dto, int userId);
        Task<CartDTO> UpdateCartAsync(UpdateCartDTO dto);
        Task<bool> DeleteCartItemAsync(int cartItemId);
        Task DeleteCartAsync(int cartId);
        Task<bool> ClearUserCartAsync(int userId);
    }
}
