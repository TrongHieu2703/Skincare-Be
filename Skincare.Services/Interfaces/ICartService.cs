using Skincare.BusinessObjects.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartDTO>> GetAllCartsAsync(int pageNumber, int pageSize);
        Task<CartDTO> GetCartByIdAsync(int id);
        Task<IEnumerable<CartDTO>> GetCartsByUserIdAsync(int userId);
        Task<CartDTO> AddCartAsync(AddToCartDTO dto, int userId);
        Task<CartDTO> UpdateCartAsync(UpdateCartDTO dto);
        Task DeleteCartAsync(int id);
    }
}
