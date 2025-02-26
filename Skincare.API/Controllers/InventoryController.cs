using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInventories() => Ok(await _inventoryService.GetAllInventoriesAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryById(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            return inventory != null ? Ok(inventory) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto dto)
        {
            var inventory = await _inventoryService.CreateInventoryAsync(dto);
            return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.InventoryId }, inventory);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryDto dto)
        {
            var updatedInventory = await _inventoryService.UpdateInventoryAsync(id, dto);
            return Ok(updatedInventory);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            await _inventoryService.DeleteInventoryAsync(id);
            return NoContent();
        }
    }
}
