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
    public class ProductTypeController : ControllerBase
    {
        private readonly IProductTypeService _productTypeService;
        private readonly ILogger<ProductTypeController> _logger;

        public ProductTypeController(
            IProductTypeService productTypeService,
            ILogger<ProductTypeController> logger)
        {
            _productTypeService = productTypeService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductTypes()
        {
            try
            {
                var productTypes = await _productTypeService.GetAllProductTypesAsync();
                return Ok(new { message = "Product types retrieved successfully", data = productTypes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product types");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductTypeById(int id)
        {
            try
            {
                var productType = await _productTypeService.GetProductTypeByIdAsync(id);
                if (productType == null)
                {
                    return NotFound(new { message = "Product type not found", errorCode = "PRODUCT_TYPE_NOT_FOUND" });
                }
                return Ok(new { message = "Product type retrieved successfully", data = productType });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product type with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
    }
} 