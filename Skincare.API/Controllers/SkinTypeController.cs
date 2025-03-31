using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkinTypeController : ControllerBase
    {
        private readonly ISkinTypeService _skinTypeService;
        private readonly ILogger<SkinTypeController> _logger;

        public SkinTypeController(
            ISkinTypeService skinTypeService,
            ILogger<SkinTypeController> logger)
        {
            _skinTypeService = skinTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSkinTypes()
        {
            try
            {
                _logger.LogInformation("Getting all skin types");
                var skinTypes = await _skinTypeService.GetAllSkinTypesAsync();
                _logger.LogInformation($"Retrieved {skinTypes} skin types");
                return Ok(new { message = "Skin types retrieved successfully", data = skinTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skin types");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSkinTypeById(int id)
        {
            try
            {
                var skinType = await _skinTypeService.GetSkinTypeByIdAsync(id);
                if (skinType == null)
                {
                    return NotFound(new { message = "Skin type not found", errorCode = "SKIN_TYPE_NOT_FOUND" });
                }
                return Ok(new { message = "Skin type retrieved successfully", data = skinType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving skin type with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        
        // Simple test endpoint that doesn't rely on the service
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                _logger.LogInformation("Test endpoint called");
                return Ok(new { message = "SkinType API is working", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, new { message = "Error in test endpoint", error = ex.Message });
            }
        }
    }
} 