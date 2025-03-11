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
        private readonly ILogger<CartService> _logger;

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        // Mapping: Entity -> DTO
        private CartDTO MapCartToDTO(Cart cart)
        {
            if (cart == null)
                return null;

            return new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity,
                AddedDate = cart.AddedDate,
                Product = cart.Product != null ? new ProductDto
                {
                    Id = cart.Product.Id,
                    Name = cart.Product.Name,
                    Description = cart.Product.Description,
                    Price = cart.Product.Price,
                    Image = cart.Product.Image,
                    IsAvailable = cart.Product.IsAvailable
                    // Các trường khác nếu cần
                } : null
            };
        }

        public async Task<IEnumerable<CartDTO>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            var carts = await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
            return carts.Select(c => MapCartToDTO(c)).ToList();
        }

        public async Task<CartDTO> GetCartByIdAsync(int id)
        {
            var cart = await _cartRepository.GetCartByIdAsync(id);
            if (cart == null)
                throw new NotFoundException($"Cart with ID {id} not found.");
            return MapCartToDTO(cart);
        }

        public async Task<IEnumerable<CartDTO>> GetCartsByUserIdAsync(int userId)
        {
            var carts = await _cartRepository.GetCartsByUserIdAsync(userId);
            return carts.Select(c => MapCartToDTO(c)).ToList();
        }

        public async Task<CartDTO> AddCartAsync(AddToCartDTO dto, int userId)
        {
            // Kiểm tra nếu đã có Cart cho sản phẩm này của user
            var existingCart = await _cartRepository.GetCartByUserAndProductAsync(userId, dto.ProductId);
            if (existingCart != null)
            {
                // Nếu đã có, cập nhật số lượng
                existingCart.Quantity += dto.Quantity;
                existingCart.AddedDate = DateTime.Now; // Cập nhật lại thời gian
                var updated = await _cartRepository.UpdateCartAsync(existingCart);
                return MapCartToDTO(updated);
            }
            else
            {
                // Nếu chưa có, tạo mới
                var cart = new Cart
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    AddedDate = DateTime.Now
                };

                var createdCart = await _cartRepository.AddCartAsync(cart);
                return MapCartToDTO(createdCart);
            }
        }

        public async Task<CartDTO> UpdateCartAsync(UpdateCartDTO dto)
        {
            var existingCart = await _cartRepository.GetCartByIdAsync(dto.CartId);
            if (existingCart == null)
                throw new NotFoundException($"Cart with ID {dto.CartId} not found.");

            existingCart.ProductId = dto.ProductId;
            existingCart.Quantity = dto.Quantity;

            var updatedCart = await _cartRepository.UpdateCartAsync(existingCart);
            return MapCartToDTO(updatedCart);
        }

        public async Task DeleteCartAsync(int id)
        {
            var existingCart = await _cartRepository.GetCartByIdAsync(id);
            if (existingCart == null)
                throw new NotFoundException($"Cart with ID {id} not found.");

            await _cartRepository.DeleteCartAsync(id);
        }
    }
}
