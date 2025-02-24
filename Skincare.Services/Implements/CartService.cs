using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using Skincare.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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

        public async Task<IEnumerable<Cart>> GetAllCartsAsync(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Fetching all carts.");
                return await _cartRepository.GetAllCartsAsync(pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all carts.");
                throw;
            }
        }

        public async Task<Cart> GetCartByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Fetching cart with ID: {id}");
                return await _cartRepository.GetCartByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching cart with ID: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<Cart>> GetCartsByUserIdAsync(int userId)
        {
            try
            {
                _logger.LogInformation($"Fetching carts for user ID: {userId}");
                return await _cartRepository.GetCartsByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching carts for user ID: {userId}");
                throw;
            }
        }

        public async Task<Cart> AddCartAsync(Cart cart)
        {
            try
            {
                _logger.LogInformation("Adding a new cart.");
                return await _cartRepository.AddCartAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding a new cart.");
                throw;
            }
        }

        public async Task UpdateCartAsync(Cart cart)
        {
            try
            {
                _logger.LogInformation($"Updating cart with ID: {cart.CartId}");
                await _cartRepository.UpdateCartAsync(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating cart with ID: {cart.CartId}");
                throw;
            }
        }

        public async Task DeleteCartAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Deleting cart with ID: {id}");
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