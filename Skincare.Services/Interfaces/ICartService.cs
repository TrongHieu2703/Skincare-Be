using Skincare.BusinessObjects.DTOs;
using System.Threading.Tasks;

namespace Skincare.Services.Interfaces
{
    public interface ICartService
    {
        // Lấy giỏ hàng của người dùng
        Task<CartDTO> GetCartsByUserIdAsync(int userId);

        // Thêm sản phẩm vào giỏ hàng
        Task<CartDTO> AddToCartAsync(int userId, AddToCartDTO dto);

        // Cập nhật số lượng sản phẩm trong giỏ hàng
        Task<CartDTO> UpdateCartItemAsync(int userId, UpdateCartItemDTO dto);

        // Xóa sản phẩm khỏi giỏ hàng
        Task<bool> RemoveFromCartAsync(int userId, int productId);

        // Xóa toàn bộ giỏ hàng
        Task<bool> ClearCartAsync(int userId);
    }
}
