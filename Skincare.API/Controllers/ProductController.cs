using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Skincare.BusinessObjects.DTOs;
using Skincare.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Skincare.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            try
            {
                var productDto = await _productService.GetProductByIdAsync(id);
                return productDto != null ? Ok(productDto) : NotFound("Product not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching product with ID {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("by-type/{productTypeId}")]
        public async Task<IActionResult> GetByType(int productTypeId)
        {
            try
            {
                var products = await _productService.GetByTypeAsync(productTypeId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching products by type {productTypeId}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            try
            {
                var products = await _productService.SearchProductsAsync(keyword);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with keyword: {keyword}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterProducts([FromQuery] string category, [FromQuery] bool? inStock, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            try
            {
                var products = await _productService.FilterProductsAsync(category, inStock, minPrice, maxPrice);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering products");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        // So sánh sản phẩm: Nhận danh sách ProductIds để so sánh
        [HttpPost("compare")]
        public async Task<IActionResult> CompareProducts([FromBody] CompareRequestDto compareRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var products = await _productService.CompareProductsAsync(compareRequest);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing products");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var createdProduct = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedProduct = await _productService.UpdateProductAsync(id, productDto);
                return updatedProduct != null ? Ok(updatedProduct) : NotFound("Product not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with ID {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with ID {id}");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}
