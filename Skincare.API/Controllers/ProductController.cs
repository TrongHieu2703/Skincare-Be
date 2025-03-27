using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Skincare.BusinessObjects.DTOs;
using Skincare.BusinessObjects.Exceptions;
using Skincare.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Skincare.Services.Implements;
using System.Text.Json;

namespace Skincare.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5); // Cache 5 phút

        private readonly IFileService _fileService;

        public ProductController(
            IProductService productService,
            ILogger<ProductController> logger,
            IMemoryCache cache,
            IFileService fileService)
        {
            _productService = productService;
            _logger = logger;
            _cache = cache;
            _fileService = fileService;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<IActionResult> GetAllProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation($"Getting products page {pageNumber}, size {pageSize}");
                var (products, totalPages, totalItems) = await _productService.GetAllProductsAsync(pageNumber, pageSize);
                _logger.LogInformation($"Retrieved {products.Count()} products, Total pages: {totalPages}, Total items: {totalItems}");
                
                var response = new { 
                    message = "Products retrieved successfully", 
                    data = products,
                    pagination = new {
                        currentPage = pageNumber,
                        pageSize = pageSize,
                        totalPages = totalPages,
                        totalItems = totalItems
                    }
                };

                _logger.LogInformation($"Sending response with pagination: {JsonSerializer.Serialize(response.pagination)}");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllProducts: {ErrorMessage}", ex.Message);
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning($"Product with ID {id} not found");
                    return NotFound(new { message = "Product not found", errorCode = "PRODUCT_NOT_FOUND" });
                }

                if (!string.IsNullOrEmpty(product.Image))
                {
                    if (!product.Image.StartsWith("/"))
                    {
                        product.Image = "/" + product.Image;
                    }
                }

                return Ok(new { message = "Product retrieved successfully", data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving product with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/search?keyword=abc
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    return Ok(new { message = "No search term provided", data = new List<ProductDto>() });
                }

                var products = await _productService.SearchProductsAsync(keyword);
                return Ok(new { message = "Products searched successfully", data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with keyword {keyword}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // POST: api/Product
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductDto createProductDto, IFormFile image = null)
        {
            try
            {
                var product = await _productService.CreateProductWithImageAsync(createProductDto, image);
                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, new { message = "Product created successfully", data = product });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDto product)
        {
            _logger.LogInformation($"Updating product with id: {id}");
            if (product == null)
            {
                _logger.LogWarning("Update product failed: Product is null");
                return BadRequest(new { success = false, message = "Product cannot be null" });
            }

            try
            {
                var result = await _productService.UpdateProductAsync(id, product);
                return Ok(new { success = true, message = "Update product successfully", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating product with id: {id}");
                return StatusCode(500, new { success = false, message = "Error updating product: " + ex.Message });
            }
        }

        [HttpPost("upload-product-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProductImage([FromForm] IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No image provided" });

            try
            {
                var filePath = await _fileService.SaveFileAsync(image, "product-images");
                return Ok(new { imageUrl = filePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading product image");
                return StatusCode(500, "Internal server error");
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                return Ok(new { message = $"Product with ID {id} deleted successfully" });
            }
            catch (NotFoundException nfex)
            {
                _logger.LogWarning(nfex, "Product not found for delete");
                return NotFound(new { message = nfex.Message, errorCode = "PRODUCT_NOT_FOUND" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting product with id {id}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // GET: api/Product/skin-type/1
        [HttpGet("skin-type/{skinTypeId}")]
        public async Task<IActionResult> GetProductsBySkinType(int skinTypeId)
        {
            try
            {
                var products = await _productService.GetProductsBySkinTypeAsync(skinTypeId);
                return Ok(new { message = "Products retrieved successfully", data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving products for skin type {skinTypeId}");
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        // Add a simple test endpoint
        [HttpGet("test")]
        public IActionResult Test()
        {
            try
            {
                _logger.LogInformation("Test endpoint called");
                return Ok(new { message = "API is working", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, new { message = "Error in test endpoint", error = ex.Message });
            }
        }
    }

    internal class _fileService
    {
    }

    // Extension method to get all keys from IMemoryCache

    public static class MemoryCacheExtensions
    {
        private static readonly Lazy<object> _keys = new Lazy<object>(() => { return new object(); });
        private static readonly Dictionary<object, HashSet<string>> _keysCollection = new Dictionary<object, HashSet<string>>();

        public static HashSet<string> GetKeys<T>(this IMemoryCache memoryCache)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            return keys;
        }

        public static void AddKey(this IMemoryCache memoryCache, string key)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            keys.Add(key);
        }

        public static void RemoveKey(this IMemoryCache memoryCache, string key)
        {
            var keys = _keysCollection.GetOrAdd(_keys.Value, _ => new HashSet<string>());
            keys.Remove(key);
        }

        private static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory(key);
                dict[key] = value;
            }
            return value;
        }
    }
}

