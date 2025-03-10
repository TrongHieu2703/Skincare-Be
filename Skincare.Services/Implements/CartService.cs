using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skincare.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        private CartDTO MapCartToDTO(Cart cart)
        {
            if (cart == null) return null;

            return new CartDTO
            {
                CartId = cart.CartId,
                AccountId = cart.AccountId,
                AddedDate = cart.AddedDate,
                SubTotal = cart.CartItems?.Sum(item => item.Product.Price * item.Quantity) ?? 0,
                CartItems = cart.CartItems?.Select(item => new CartItemDTO
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "Unknown",
                    Price = item.Product?.Price ?? 0,
                    Quantity = item.Quantity,
                    Image = item.Product?.MainImage,
                    TotalPrice = (item.Product?.Price ?? 0) * item.Quantity
                }).ToList() ?? new List<CartItemDTO>()
            };
        }

        public async Task<IEnumerable<Cart>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            //var carts = await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
            //return carts.Select(c => MapCartToDTO(c)).ToList();
            return await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
        }

        public async Task<CartDTO> GetCartByIdAsync(int id)
        {

            var cart = await _cartRepository.GetCartByIdAsync(id);
            return MapCartToDTO(cart);
        }

        public async Task<CartResponseDTO> GetCartsByUserIdAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        AccountId = userId,
                        AddedDate = DateTime.Now
                    };
                    await _cartRepository.AddCartAsync(cart);
                }
                return CartResponseDTO.CreateSuccess("Lấy giỏ hàng thành công", MapCartToDTO(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy giỏ hàng của user {UserId}", userId);
                return CartResponseDTO.CreateError("Không thể lấy giỏ hàng. Vui lòng thử lại sau.");
            }
        }

        public async Task<CartResponseDTO> AddToCartAsync(int userId, AddToCartDTO dto)
        {
            try
            {
                var cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                if (cart == null)
                {
                    cart = new Cart
                    {
                        AccountId = userId,
                        AddedDate = DateTime.Now
                    };
                    await _cartRepository.AddCartAsync(cart);
                }

                var cartItem = await _cartRepository.GetCartItemAsync(cart.CartId, dto.ProductId);
                if (cartItem != null)
                {
                    cartItem.Quantity += dto.Quantity;
                    await _cartRepository.UpdateCartItemAsync(cartItem);
                }
                else
                {
                    cartItem = new CartItem
                    {
                        CartId = cart.CartId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity
                    };
                    await _cartRepository.AddCartItemAsync(cartItem);
                }

                cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                return CartResponseDTO.CreateSuccess("Thêm vào giỏ hàng thành công", MapCartToDTO(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng cho user {UserId}", userId);
                return CartResponseDTO.CreateError("Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại sau.");
            }
        }

        public async Task<CartResponseDTO> UpdateCartItemAsync(int userId, UpdateCartItemDTO dto)
        {
            try
            {
                var cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                if (cart == null)
                    return CartResponseDTO.CreateError("Giỏ hàng không tồn tại");

                var cartItem = await _cartRepository.GetCartItemAsync(cart.CartId, dto.ProductId);
                if (cartItem == null)
                    return CartResponseDTO.CreateError("Sản phẩm không tồn tại trong giỏ hàng");

                if (dto.Quantity <= 0)
                {
                    await _cartRepository.DeleteCartItemAsync(cart.CartId, dto.ProductId);
                }
                else
                {
                    cartItem.Quantity = dto.Quantity;
                    await _cartRepository.UpdateCartItemAsync(cartItem);
                }

                cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                return CartResponseDTO.CreateSuccess("Cập nhật giỏ hàng thành công", MapCartToDTO(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giỏ hàng cho user {UserId}", userId);
                return CartResponseDTO.CreateError("Không thể cập nhật giỏ hàng. Vui lòng thử lại sau.");
            }
        }

        public async Task<CartResponseDTO> RemoveFromCartAsync(int userId, int productId)
        {
            try
            {
                var cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                if (cart == null)
                    return CartResponseDTO.CreateError("Giỏ hàng không tồn tại");

                var success = await _cartRepository.DeleteCartItemAsync(cart.CartId, productId);
                if (!success)
                    return CartResponseDTO.CreateError("Không thể xóa sản phẩm khỏi giỏ hàng");

                cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                return CartResponseDTO.CreateSuccess("Đã xóa sản phẩm khỏi giỏ hàng", MapCartToDTO(cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng cho user {UserId}", userId);
                return CartResponseDTO.CreateError("Không thể xóa sản phẩm khỏi giỏ hàng. Vui lòng thử lại sau.");
            }
        }

        public async Task<CartResponseDTO> ClearCartAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartsByUserIdAsync(userId);
                if (cart == null)
                    return CartResponseDTO.CreateError("Giỏ hàng không tồn tại");

                var success = await _cartRepository.ClearCartAsync(cart.CartId);
                if (!success)
                    return CartResponseDTO.CreateError("Không thể xóa giỏ hàng");

                return CartResponseDTO.CreateSuccess("Đã xóa tất cả sản phẩm khỏi giỏ hàng");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giỏ hàng cho user {UserId}", userId);
                return CartResponseDTO.CreateError("Không thể xóa giỏ hàng. Vui lòng thử lại sau.");
            }
        }

        public async Task DeleteCartAsync(int id)
        {
            await _cartRepository.DeleteCartAsync(id);
        }
    }
}
