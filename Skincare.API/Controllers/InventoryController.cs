using Microsoft.AspNetCore.Mvc;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions; // import NotFoundException
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> GetAllInventories()
        {
            try
            {
                var inventories = await _inventoryService.GetAllInventoriesAsync();
                return Ok(new { message = "Fetched inventories successfully", data = inventories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInventoryById(int id)
        {
            try
            {
                var inventory = await _inventoryService.GetInventoryByIdAsync(id);
                return Ok(new { message = "Fetched inventory successfully", data = inventory });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "INVENTORY_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateInventory([FromBody] CreateInventoryDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Inventory data is null" });
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid request data", errors = ModelState });
            try
            {
                var inventory = await _inventoryService.CreateInventoryAsync(dto);
                return CreatedAtAction(nameof(GetInventoryById), new { id = inventory.Id }, new { message = "Inventory created successfully", data = inventory });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] UpdateInventoryDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Update data is null" });
            try
            {
                var updatedInventory = await _inventoryService.UpdateInventoryAsync(id, dto);
                return Ok(new { message = "Inventory updated successfully", data = updatedInventory });
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "INVENTORY_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            try
            {
                await _inventoryService.DeleteInventoryAsync(id);
                return NoContent();
            }
            catch (NotFoundException nfex)
            {
                return NotFound(new { message = nfex.Message, errorCode = "INVENTORY_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }
    }
}
