using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Entities;
using Skincare.Repositories.Interfaces;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Logging;
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
        private CartDTO MapCartToDTO(Cart cart)
        {
            if (cart == null) return null;

            return new CartDTO
            {
                CartId = cart.CartId,
                UserId = cart.UserId,
                ProductId = cart.ProductId,
                Quantity = cart.Quantity,
                AddedDate = cart.AddedDate,
                Product = cart.Product == null ? null : new ProductDTO
                {
                    Id = cart.Product.Id,
                    Name = cart.Product.Name,
                    Price = cart.Product.Price,
                    Description = cart.Product.Description,
                    Image = cart.Product.Image,
                    IsAvailable = cart.Product.IsAvailable
                }
            };
        }

        public CartService(ICartRepository cartRepository, ILogger<CartService> logger)
        {
            _cartRepository = cartRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CartDTO>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching all carts.");
                var cartEntities = await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
                return cartEntities.Select(MapCartToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all carts.");
                throw;
            }
        }

        public async Task<CartDTO> GetCartByIdAsync(int id)
        {
            try
            {
                var cartEntity = await _cartRepository.GetCartByIdAsync(id);
                return cartEntity == null ? null : MapCartToDTO(cartEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching cart with ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<CartDTO>> GetCartsByUserIdAsync(int userId)
        {
            try
            {
                var cartEntities = await _cartRepository.GetCartsByUserIdAsync(userId);
                return cartEntities.Select(MapCartToDTO).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching carts for user ID: {userId}");
                throw;
            }
        }

        public async Task<CartDTO> AddCartAsync(AddToCartDTO dto, int userId)
        {
            try
            {
                var cartEntity = new Cart
                {
                    UserId = userId,
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    AddedDate = DateTime.Now
                };

                var createdCart = await _cartRepository.AddCartAsync(cartEntity);
                return MapCartToDTO(createdCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new cart.");
                throw;
            }
        }

        public async Task<CartDTO> UpdateCartAsync(UpdateCartDTO dto)
        {
            try
            {
                var existingCart = await _cartRepository.GetCartByIdAsync(dto.CartId);
                if (existingCart == null) return null;

                existingCart.ProductId = dto.ProductId;
                existingCart.Quantity = dto.Quantity;

                await _cartRepository.UpdateCartAsync(existingCart);
                return MapCartToDTO(existingCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating cart with ID: {dto.CartId}");
                throw;
            }
        }

        public async Task DeleteCartAsync(int id)
        {
            try
            {
                await _cartRepository.DeleteCartAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting cart with ID: {id}");
                throw;
            }
        }
    }
}
