using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.Entities;
using Skincare.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            try
            {
                var carts = await _cartService.GetAllCartsAsync();
                return Ok(carts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all carts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            try
            {
                var cart = await _cartService.GetCartByIdAsync(id);
                if (cart == null)
                    return NotFound();

                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cart with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartsByUserId(int userId)
        {
            try
            {
                var carts = await _cartService.GetCartsByUserIdAsync(userId);
                return Ok(carts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching carts for user ID {userId}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCart([FromBody] Cart cart)
        {
            if (cart == null)
                return BadRequest("Cart is null");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdCart = await _cartService.AddCartAsync(cart);
                return CreatedAtAction(nameof(GetCartById), new { id = createdCart.CartId }, createdCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cart");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, [FromBody] Cart cart)
        {
            if (cart == null || cart.CartId != id)
                return BadRequest("Cart ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _cartService.UpdateCartAsync(cart);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            try
            {
                await _cartService.DeleteCartAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}