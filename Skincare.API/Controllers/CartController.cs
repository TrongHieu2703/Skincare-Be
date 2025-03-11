using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import custom exceptions
using Skincare.Services.Interfaces;
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
                return Ok(new { message = "Fetched all carts successfully", data = carts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all carts");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            try
            {
                var cart = await _cartService.GetCartByIdAsync(id);
                return Ok(new { message = "Fetched cart successfully", data = cart });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Cart not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "CART_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cart with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetCartsByUserId()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var carts = await _cartService.GetCartsByUserIdAsync(userId);
                return Ok(new { message = "Fetched carts for user successfully", data = carts });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching carts for user");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddCart([FromBody] AddToCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(new { message = "Invalid user token" });

            try
            {
                var createdCart = await _cartService.AddCartAsync(dto, userId);
                return CreatedAtAction(nameof(GetCartById), new { id = createdCart.CartId }, new { message = "Cart added successfully", data = createdCart });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding cart");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, [FromBody] UpdateCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });

            if (id != dto.CartId)
                return BadRequest(new { message = "Cart ID mismatch" });

            try
            {
                var updatedCart = await _cartService.UpdateCartAsync(dto);
                return Ok(new { message = "Cart updated successfully", data = updatedCart });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Cart not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "CART_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
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
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Cart not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "CART_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
