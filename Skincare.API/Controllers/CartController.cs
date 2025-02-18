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

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCarts()
        {
            var carts = await _cartService.GetAllCartsAsync();
            return Ok(carts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            var cart = await _cartService.GetCartByIdAsync(id);
            if (cart == null)
                return NotFound();
            return Ok(cart);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartsByUserId(int userId)
        {
            var carts = await _cartService.GetCartsByUserIdAsync(userId);
            return Ok(carts);
        }

        [HttpPost]
        public async Task<IActionResult> AddCart([FromBody] Cart cart)
        {
            if (cart == null)
                return BadRequest();

            var createdCart = await _cartService.AddCartAsync(cart);
            return CreatedAtAction(nameof(GetCartById), new { id = createdCart.CartId }, createdCart);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCart(int id, [FromBody] Cart cart)
        {
            if (cart == null || id != cart.CartId)
                return BadRequest();

            await _cartService.UpdateCartAsync(cart);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            await _cartService.DeleteCartAsync(id);
            return NoContent();
        }
    }
}
