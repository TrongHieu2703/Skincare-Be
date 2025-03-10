using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
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
        public async Task<IActionResult> GetUserCart()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var result = await _cartService.GetCartsByUserIdAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy giỏ hàng của người dùng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var result = await _cartService.AddToCartAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCartItem([FromBody] UpdateCartItemDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var result = await _cartService.UpdateCartItemAsync(userId, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật giỏ hàng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var result = await _cartService.RemoveFromCartAsync(userId, productId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa sản phẩm khỏi giỏ hàng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { message = "Token không hợp lệ" });

                var result = await _cartService.ClearCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa giỏ hàng");
                return StatusCode(500, new { message = "Đã xảy ra lỗi khi xử lý yêu cầu" });
            }
        }
    }
}
