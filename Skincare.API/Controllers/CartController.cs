using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Yêu cầu xác thực JWT
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
        public async Task<IActionResult> GetAllCarts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var carts = await _cartService.GetAllCartsAsync(pageNumber, pageSize);
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
                    return NotFound("Cart not found");

                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cart with ID {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetCartsByUserId()
        {
            try
            {
                var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdString, out var userId))
                    return Unauthorized("Invalid user token");

                var carts = await _cartService.GetCartsByUserIdAsync(userId);
                return Ok(carts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching carts for user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCart([FromBody] AddToCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out var userId))
                return Unauthorized("Invalid user token");

            try
            {
                var createdCart = await _cartService.AddCartAsync(dto, userId);
                return CreatedAtAction(nameof(GetCartById), new { id = createdCart.CartId }, createdCart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cart");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, [FromBody] UpdateCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (id != dto.CartId)
                return BadRequest("Cart ID mismatch");

            try
            {
                var updatedCart = await _cartService.UpdateCartAsync(dto);
                if (updatedCart == null)
                    return NotFound("Cart not found");

                return Ok(updatedCart);
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
