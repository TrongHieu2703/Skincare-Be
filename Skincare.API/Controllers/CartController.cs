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

                var cart = await _cartService.GetCartByUserIdAsync(userId);
                return Ok(new { message = "Fetched cart for user successfully", data = cart });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching cart for user");
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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Inventory validation failed");
                return BadRequest(new { message = ex.Message, errorCode = "INSUFFICIENT_INVENTORY" });
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
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Inventory validation failed");
                return BadRequest(new { message = ex.Message, errorCode = "INSUFFICIENT_INVENTORY" });
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

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearUserCart()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Invalid user token" });

                var success = await _cartService.ClearUserCartAsync(userId);
                return Ok(new { message = "Cart cleared successfully", success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing user cart");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        } 

        [HttpGet("item/{id}")]
        public async Task<IActionResult> GetCartItemById(int id)
        {
            try
            {
                var cartItem = await _cartService.GetCartItemByIdAsync(id);
                return Ok(new { message = "Fetched cart item successfully", data = cartItem });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Cart item not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "CART_ITEM_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching cart item with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut("item/{id}")]
        public async Task<IActionResult> UpdateCartItem(int id, [FromBody] UpdateCartItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });

            if (id != dto.CartItemId)
                return BadRequest(new { message = "Cart item ID mismatch" });

            try
            {
                var updated = await _cartService.UpdateCartItemAsync(id, dto.Quantity);
                return Ok(new { message = "Cart item updated successfully", success = updated });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, $"Cart item not found with ID {id}");
                return NotFound(new { message = nfex.Message, errorCode = "CART_ITEM_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating cart item with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpDelete("item/{id}")]
        public async Task<IActionResult> DeleteCartItem(int id)
        {
            try
            {
                var success = await _cartService.DeleteCartItemAsync(id);
                if (success)
                    return NoContent();
                return NotFound(new { message = $"Cart item with ID {id} not found", errorCode = "CART_ITEM_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting cart item with ID {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
}
