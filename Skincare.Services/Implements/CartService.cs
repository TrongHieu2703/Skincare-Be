using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.BusinessObjects.Exceptions;
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
        private readonly IProductRepository _productRepository;
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CartDTO>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            try
            {
                var carts = await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
                return carts.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all carts");
                throw;
            }
        }

        public async Task<CartDTO> GetCartByIdAsync(int id)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(id);
                if (cart == null)
                    throw new NotFoundException($"Cart with ID {id} not found");

                return MapToDto(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart with ID {id}");
                throw;
            }
        }

        public async Task<CartDTO> GetCartByUserIdAsync(int userId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                return MapToDto(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting cart for user ID {userId}");
                throw;
            }
        }

        public async Task<CartDTO> AddCartAsync(AddToCartDTO dto, int userId)
        {
            try
            {
                // Kiểm tra sản phẩm tồn tại
                var product = await _productRepository.GetProductByIdAsync(dto.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {dto.ProductId} not found");

                // Lấy hoặc tạo giỏ hàng cho user
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);

                // Thêm sản phẩm vào giỏ hàng
                var cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                await _cartRepository.AddItemToCartAsync(cartItem);

                // Lấy giỏ hàng đã cập nhật
                var updatedCart = await _cartRepository.GetCartByIdAsync(cart.CartId);
                return MapToDto(updatedCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                throw;
            }
        }

        public async Task<CartDTO> UpdateCartAsync(UpdateCartDTO dto)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(dto.CartId);
                if (cart == null)
                    throw new NotFoundException($"Cart with ID {dto.CartId} not found");

                // Kiểm tra sản phẩm tồn tại
                var product = await _productRepository.GetProductByIdAsync(dto.ProductId);
                if (product == null)
                    throw new NotFoundException($"Product with ID {dto.ProductId} not found");

                // Cập nhật số lượng sản phẩm
                var cartItem = new CartItem
                {
                    CartId = dto.CartId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                };

                if (dto.Quantity <= 0)
                {
                    // Nếu số lượng <= 0, xóa sản phẩm khỏi giỏ hàng
                    var existingItem = await _cartRepository.GetCartItemAsync(dto.CartId, dto.ProductId);
                    if (existingItem != null)
                    {
                        await _cartRepository.DeleteCartItemAsync(existingItem.Id);
                    }
                }
                else
                {
                    // Cập nhật số lượng
                    await _cartRepository.UpdateCartItemAsync(cartItem);
                }

                // Lấy giỏ hàng đã cập nhật
                var updatedCart = await _cartRepository.GetCartByIdAsync(dto.CartId);
                return MapToDto(updatedCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart");
                throw;
            }
        }

        public async Task<bool> DeleteCartItemAsync(int cartItemId)
        {
            try
            {
                return await _cartRepository.DeleteCartItemAsync(cartItemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart item with ID {cartItemId}");
                throw;
            }
        }

        public async Task DeleteCartAsync(int cartId)
        {
            try
            {
                var cart = await _cartRepository.GetCartByIdAsync(cartId);
                if (cart == null)
                    throw new NotFoundException($"Cart with ID {cartId} not found");

                // Xóa tất cả các CartItem trước
                if (cart.CartItems != null && cart.CartItems.Any())
                {
                    foreach (var item in cart.CartItems.ToList())
                    {
                        await _cartRepository.DeleteCartItemAsync(item.Id);
                    }
                }

                // Sau đó xóa Cart
                await _cartRepository.DeleteCartAsync(cartId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart with ID {cartId}");
                throw;
            }
        }

        public async Task<bool> ClearUserCartAsync(int userId)
        {
            try
            {
                // Lấy giỏ hàng của người dùng
                var cart = await _cartRepository.GetCartByUserIdAsync(userId);
                if (cart == null || cart.CartItems == null || !cart.CartItems.Any())
                    return true; // Giỏ hàng trống hoặc không tồn tại - không cần xóa

                // Xóa tất cả các mục trong giỏ hàng
                foreach (var item in cart.CartItems.ToList())
                {
                    await _cartRepository.DeleteCartItemAsync(item.Id);
                }

                _logger.LogInformation($"Cleared all items in cart for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error clearing cart for user ID {userId}");
                throw;
            }
        }

        private CartDTO MapToDto(Cart cart)
        {
            decimal totalPrice = 0;
            var cartItemDtos = new List<CartItemDTO>();

            if (cart.CartItems != null)
            {
                foreach (var item in cart.CartItems)
                {
                    var price = item.Product?.Price ?? 0;
                    var quantity = item.Quantity ?? 0;
                    totalPrice += price * quantity;

                    cartItemDtos.Add(new CartItemDTO
                    {
                        Id = item.Id,
                        CartId = item.CartId ?? 0,
                        ProductId = item.ProductId ?? 0,
                        Quantity = item.Quantity ?? 0,
                        ProductName = item.Product?.Name ?? "Unknown Product",
                        ProductImage = item.Product?.Image,
                        ProductPrice = price
                    });
                }
            }

            return new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                CartItems = cartItemDtos,
                TotalPrice = totalPrice
            };
        }
    }
}
